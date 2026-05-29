using AgentFlow.Domain.Executions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentFlow.Infrastructure.Persistence.Configurations;

internal sealed class ExecutionStepConfiguration : IEntityTypeConfiguration<ExecutionStep>
{
    public void Configure(EntityTypeBuilder<ExecutionStep> builder)
    {
        builder.ToTable("execution_steps", "workflow");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.WorkflowExecutionId)
            .HasColumnName("workflow_execution_id")
            .IsRequired();

        builder.Property(s => s.NodeId)
            .HasColumnName("node_id")
            .IsRequired();

        builder.Property(s => s.NodeType)
            .HasColumnName("node_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.NodeName)
            .HasColumnName("node_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Sequence)
            .HasColumnName("sequence")
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(s => s.Input)
            .HasColumnName("input")
            .HasColumnType("jsonb");

        builder.Property(s => s.Output)
            .HasColumnName("output")
            .HasColumnType("jsonb");

        builder.Property(s => s.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(4000);

        builder.Property(s => s.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(s => s.StartedAtUtc)
            .HasColumnName("started_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(s => s.CompletedAtUtc)
            .HasColumnName("completed_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Ignore(s => s.DomainEvents);
    }
}
