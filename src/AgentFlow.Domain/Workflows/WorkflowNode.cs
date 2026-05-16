using AgentFlow.Domain.SharedKernel;

namespace AgentFlow.Domain.Workflows;

public sealed class WorkflowNode : Entity<Guid>
{
    private const int TypeMaxLength = 100;
    private const int NameMaxLength = 200;

    private WorkflowNode()
        : base(Guid.Empty)
    {
    }

    private WorkflowNode(
        Guid id,
        Guid workflowId,
        string type,
        string name,
        double positionX,
        double positionY,
        string configuration)
        : base(id)
    {
        WorkflowId = Guard.NotEmpty(workflowId, nameof(workflowId));
        Type = Guard.NotBlank(type, nameof(type), TypeMaxLength);
        Name = Guard.NotBlank(name, nameof(name), NameMaxLength);
        PositionX = positionX;
        PositionY = positionY;
        Configuration = configuration;
    }

    public Guid WorkflowId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public double PositionX { get; private set; }
    public double PositionY { get; private set; }
    public string Configuration { get; private set; } = "{}";

    internal static WorkflowNode Create(
        Guid workflowId,
        string type,
        string name,
        double positionX,
        double positionY,
        string configuration)
    {
        return new WorkflowNode(Guid.NewGuid(), workflowId, type, name, positionX, positionY, configuration);
    }

    internal void Update(string name, double positionX, double positionY, string configuration)
    {
        Name = Guard.NotBlank(name, nameof(name), NameMaxLength);
        PositionX = positionX;
        PositionY = positionY;
        Configuration = configuration;
    }
}
