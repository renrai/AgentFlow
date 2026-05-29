using AgentFlow.Domain.Executions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentFlow.Infrastructure.Persistence.Configurations;

internal sealed class WorkflowExecutionConfiguration : IEntityTypeConfiguration<WorkflowExecution>
{
    public void Configure(EntityTypeBuilder<WorkflowExecution> builder)
    {
        builder.ToTable("workflow_executions", "workflow");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(e => e.WorkflowId)
            .HasColumnName("workflow_id")
            .IsRequired();

        builder.Property(e => e.WorkflowVersion)
            .HasColumnName("workflow_version")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.TriggerPayload)
            .HasColumnName("trigger_payload")
            .HasColumnType("jsonb");

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(4000);

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.StartedAtUtc)
            .HasColumnName("started_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.CompletedAtUtc)
            .HasColumnName("completed_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(e => new { e.TenantId, e.WorkflowId });
        builder.HasIndex(e => e.Status);

        builder.HasMany(e => e.Steps)
            .WithOne()
            .HasForeignKey(s => s.WorkflowExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Steps)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(e => e.DomainEvents);
    }
}
