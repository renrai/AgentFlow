using AgentFlow.Domain.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentFlow.Infrastructure.Persistence.Configurations;

internal sealed class WorkflowEdgeConfiguration : IEntityTypeConfiguration<WorkflowEdge>
{
    public void Configure(EntityTypeBuilder<WorkflowEdge> builder)
    {
        builder.ToTable("workflow_edges", "workflow");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(e => e.WorkflowId)
            .HasColumnName("workflow_id")
            .IsRequired();

        builder.Property(e => e.SourceNodeId)
            .HasColumnName("source_node_id")
            .IsRequired();

        builder.Property(e => e.TargetNodeId)
            .HasColumnName("target_node_id")
            .IsRequired();

        builder.Property(e => e.Label)
            .HasColumnName("label")
            .HasMaxLength(100);

        builder.Ignore(e => e.DomainEvents);
    }
}
