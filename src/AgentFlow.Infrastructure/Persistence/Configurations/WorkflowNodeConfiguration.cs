using AgentFlow.Domain.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentFlow.Infrastructure.Persistence.Configurations;

internal sealed class WorkflowNodeConfiguration : IEntityTypeConfiguration<WorkflowNode>
{
    public void Configure(EntityTypeBuilder<WorkflowNode> builder)
    {
        builder.ToTable("workflow_nodes", "workflow");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(n => n.WorkflowId)
            .HasColumnName("workflow_id")
            .IsRequired();

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.PositionX)
            .HasColumnName("position_x")
            .IsRequired();

        builder.Property(n => n.PositionY)
            .HasColumnName("position_y")
            .IsRequired();

        builder.Property(n => n.Configuration)
            .HasColumnName("configuration")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'")
            .IsRequired();

        builder.Ignore(n => n.DomainEvents);
    }
}
