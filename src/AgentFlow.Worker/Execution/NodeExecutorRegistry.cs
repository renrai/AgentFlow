namespace AgentFlow.Worker.Execution;

public interface INodeExecutorRegistry
{
    INodeExecutor Resolve(string nodeType);
}

internal sealed class NodeExecutorRegistry : INodeExecutorRegistry
{
    private readonly Dictionary<string, INodeExecutor> _executors;
    private readonly INodeExecutor _fallback;

    public NodeExecutorRegistry(IEnumerable<INodeExecutor> executors)
    {
        _executors = executors.ToDictionary(e => e.NodeType, StringComparer.OrdinalIgnoreCase);

        if (!_executors.TryGetValue("noop", out var fallback))
            throw new InvalidOperationException("A 'noop' node executor must be registered as the fallback.");

        _fallback = fallback;
    }

    public INodeExecutor Resolve(string nodeType)
        => _executors.TryGetValue(nodeType, out var executor) ? executor : _fallback;
}
