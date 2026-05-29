namespace AgentFlow.Worker.Execution;

public interface INodeExecutor
{
    string NodeType { get; }

    Task<NodeExecutionResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken);
}
