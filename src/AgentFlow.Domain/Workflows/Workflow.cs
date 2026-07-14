using System.Security.Cryptography;
using AgentFlow.Domain.SharedKernel;

namespace AgentFlow.Domain.Workflows;

public sealed class Workflow : AggregateRoot<Guid>
{
    private const int NameMaxLength = 200;
    private const string WebhookTokenPrefix = "wh_";

    private readonly List<WorkflowNode> _nodes = [];
    private readonly List<WorkflowEdge> _edges = [];

    private Workflow()
        : base(Guid.Empty)
    {
    }

    private Workflow(
        Guid id,
        Guid tenantId,
        string name,
        string? description,
        string webhookToken,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = Guard.NotEmpty(tenantId, nameof(tenantId));
        Name = Guard.NotBlank(name, nameof(name), NameMaxLength);
        Description = description;
        WebhookToken = webhookToken;
        Status = WorkflowStatus.Draft;
        Version = 1;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string WebhookToken { get; private set; } = string.Empty;
    public WorkflowStatus Status { get; private set; }
    public int Version { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<WorkflowNode> Nodes => _nodes.AsReadOnly();
    public IReadOnlyCollection<WorkflowEdge> Edges => _edges.AsReadOnly();

    public static Workflow Create(Guid tenantId, string name, string? description, DateTimeOffset utcNow)
    {
        Guard.NotEmpty(tenantId, nameof(tenantId));
        return new Workflow(Guid.NewGuid(), tenantId, name, description, GenerateWebhookToken(), utcNow);
    }

    public void RotateWebhookToken(DateTimeOffset utcNow)
    {
        EnsureNotArchived();
        WebhookToken = GenerateWebhookToken();
        UpdatedAtUtc = utcNow;
    }

    private static string GenerateWebhookToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return WebhookTokenPrefix + Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public void UpdateDefinition(
        string name,
        string? description,
        IEnumerable<NodeDefinition> nodes,
        IEnumerable<EdgeDefinition> edges,
        DateTimeOffset utcNow)
    {
        EnsureNotArchived();

        Name = Guard.NotBlank(name, nameof(name), NameMaxLength);
        Description = description;
        UpdatedAtUtc = utcNow;

        _nodes.Clear();
        _edges.Clear();

        foreach (var n in nodes)
        {
            _nodes.Add(WorkflowNode.Create(Id, n.Type, n.Name, n.PositionX, n.PositionY, n.Configuration));
        }

        foreach (var e in edges)
        {
            EnsureNodeExists(e.SourceNodeId, "source");
            EnsureNodeExists(e.TargetNodeId, "target");
            _edges.Add(WorkflowEdge.Create(Id, e.SourceNodeId, e.TargetNodeId, e.Label));
        }
    }

    public void Publish(DateTimeOffset utcNow)
    {
        EnsureNotArchived();

        if (_nodes.Count == 0)
            throw new DomainException("Cannot publish a workflow with no nodes.");

        if (Status == WorkflowStatus.Published)
            Version++;

        Status = WorkflowStatus.Published;
        UpdatedAtUtc = utcNow;
    }

    public void Archive(DateTimeOffset utcNow)
    {
        if (Status == WorkflowStatus.Archived)
            throw new DomainException("Workflow is already archived.");

        Status = WorkflowStatus.Archived;
        UpdatedAtUtc = utcNow;
    }

    private void EnsureNotArchived()
    {
        if (Status == WorkflowStatus.Archived)
            throw new DomainException("Cannot modify an archived workflow.");
    }

    private void EnsureNodeExists(Guid nodeId, string role)
    {
        if (_nodes.All(n => n.Id != nodeId))
            throw new DomainException($"The {role} node '{nodeId}' does not belong to this workflow.");
    }
}

public sealed record NodeDefinition(
    string Type,
    string Name,
    double PositionX,
    double PositionY,
    string Configuration);

public sealed record EdgeDefinition(
    Guid SourceNodeId,
    Guid TargetNodeId,
    string? Label);
