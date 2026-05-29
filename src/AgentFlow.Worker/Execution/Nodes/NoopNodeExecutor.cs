using AgentFlow.Worker.Execution;

namespace AgentFlow.Worker.Execution.Nodes;

internal sealed class NoopNodeExecutor : INodeExecutor
{
    public string NodeType => "noop";

    public Task<NodeExecutionResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
        => Task.FromResult(new NodeExecutionResult(context.Input ?? "{}"));
}
