namespace AgentFlow.Worker.Execution;

public interface IWorkflowExecutor
{
    Task ExecuteAsync(Guid executionId, CancellationToken cancellationToken);
}
