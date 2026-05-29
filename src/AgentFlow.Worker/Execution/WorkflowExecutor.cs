using System.Text.Json;
using AgentFlow.Application.Abstractions.Clock;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Executions;
using AgentFlow.Domain.Workflows;
using Microsoft.Extensions.Logging;

namespace AgentFlow.Worker.Execution;

internal sealed class WorkflowExecutor(
    IExecutionRepository executions,
    IWorkflowRepository workflows,
    IUnitOfWork unitOfWork,
    INodeExecutorRegistry registry,
    IClock clock,
    ILogger<WorkflowExecutor> logger) : IWorkflowExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task ExecuteAsync(Guid executionId, CancellationToken cancellationToken)
    {
        var execution = await executions.GetByIdAsync(executionId, cancellationToken).ConfigureAwait(false);
        if (execution is null)
        {
            LogExecutionMissing(logger, executionId, null);
            return;
        }

        if (execution.Status != ExecutionStatus.Pending)
        {
            LogExecutionSkipped(logger, executionId, execution.Status.ToString(), null);
            return;
        }

        var workflow = await workflows
            .GetByIdAsync(execution.WorkflowId, execution.TenantId, cancellationToken)
            .ConfigureAwait(false);

        if (workflow is null)
        {
            execution.MarkFailed("Workflow not found.", clock.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        execution.MarkRunning(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var sortedNodes = TopologicalSort(workflow.Nodes, workflow.Edges);
            var nodeOutputs = new Dictionary<Guid, string>();

            foreach (var node in sortedNodes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var step = execution.AddStep(node.Id, node.Type, node.Name, clock.UtcNow);
                var input = BuildNodeInput(node, workflow.Edges, nodeOutputs, execution.TriggerPayload);

                step.MarkRunning(input, clock.UtcNow);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    var executor = registry.Resolve(node.Type);
                    var context = new NodeExecutionContext(
                        execution.Id,
                        execution.TenantId,
                        execution.WorkflowId,
                        node.Id,
                        node.Type,
                        node.Name,
                        node.Configuration,
                        input);

                    var result = await executor.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
                    var output = result.Output ?? "{}";
                    nodeOutputs[node.Id] = output;
                    step.MarkCompleted(output, clock.UtcNow);
                    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    LogNodeFailed(logger, node.Id, node.Type, ex);
                    step.MarkFailed(Truncate(ex.Message, 4000), clock.UtcNow);
                    execution.MarkFailed($"Node '{node.Name}' failed: {Truncate(ex.Message, 1000)}", clock.UtcNow);
                    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    return;
                }
            }

            execution.MarkCompleted(clock.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            LogExecutionCompleted(logger, executionId, null);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogExecutionFailed(logger, executionId, ex);
            execution.MarkFailed(Truncate(ex.Message, 4000), clock.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static string BuildNodeInput(
        WorkflowNode node,
        IReadOnlyCollection<WorkflowEdge> edges,
        Dictionary<Guid, string> outputs,
        string? triggerPayload)
    {
        var incoming = edges.Where(e => e.TargetNodeId == node.Id).ToList();
        if (incoming.Count == 0)
            return triggerPayload ?? "{}";

        var payload = new Dictionary<string, JsonElement>();
        foreach (var edge in incoming)
        {
            if (outputs.TryGetValue(edge.SourceNodeId, out var upstreamOutput))
            {
                var key = edge.SourceNodeId.ToString();
                try
                {
                    using var doc = JsonDocument.Parse(upstreamOutput);
                    payload[key] = doc.RootElement.Clone();
                }
                catch (JsonException)
                {
                    payload[key] = JsonSerializer.SerializeToElement(upstreamOutput, JsonOptions);
                }
            }
        }

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static List<WorkflowNode> TopologicalSort(
        IReadOnlyCollection<WorkflowNode> nodes,
        IReadOnlyCollection<WorkflowEdge> edges)
    {
        var inDegree = nodes.ToDictionary(n => n.Id, _ => 0);
        foreach (var edge in edges)
        {
            if (inDegree.TryGetValue(edge.TargetNodeId, out var current))
                inDegree[edge.TargetNodeId] = current + 1;
        }

        var queue = new Queue<WorkflowNode>(nodes.Where(n => inDegree[n.Id] == 0).OrderBy(n => n.Name));
        var nodesById = nodes.ToDictionary(n => n.Id);
        var sorted = new List<WorkflowNode>();

        while (queue.TryDequeue(out var current))
        {
            sorted.Add(current);
            foreach (var edge in edges.Where(e => e.SourceNodeId == current.Id))
            {
                if (--inDegree[edge.TargetNodeId] == 0 && nodesById.TryGetValue(edge.TargetNodeId, out var next))
                    queue.Enqueue(next);
            }
        }

        if (sorted.Count != nodes.Count)
            throw new InvalidOperationException("Workflow contains a cycle and cannot be executed.");

        return sorted;
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];

    private static readonly Action<ILogger, Guid, Exception?> LogExecutionMissing =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(7000, nameof(LogExecutionMissing)),
            "Execution {ExecutionId} not found");

    private static readonly Action<ILogger, Guid, string, Exception?> LogExecutionSkipped =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Warning,
            new EventId(7001, nameof(LogExecutionSkipped)),
            "Execution {ExecutionId} skipped — current status: {Status}");

    private static readonly Action<ILogger, Guid, Exception?> LogExecutionCompleted =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(7002, nameof(LogExecutionCompleted)),
            "Execution {ExecutionId} completed successfully");

    private static readonly Action<ILogger, Guid, Exception?> LogExecutionFailed =
        LoggerMessage.Define<Guid>(
            LogLevel.Error,
            new EventId(7003, nameof(LogExecutionFailed)),
            "Execution {ExecutionId} failed with unhandled exception");

    private static readonly Action<ILogger, Guid, string, Exception?> LogNodeFailed =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Error,
            new EventId(7004, nameof(LogNodeFailed)),
            "Node {NodeId} ({NodeType}) failed");
}
