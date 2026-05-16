using AgentFlow.Domain.Tenants;
using AgentFlow.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentFlow.Infrastructure.Persistence.Configurations;

internal sealed class TenantMemberConfiguration : IEntityTypeConfiguration<TenantMember>
{
    public void Configure(EntityTypeBuilder<TenantMember> builder)
    {
        builder.ToTable("tenant_members", "identity");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(member => member.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(member => member.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(member => new { member.TenantId, member.UserId })
            .IsUnique()
            .HasFilter("status <> 'Removed'");

        builder.HasIndex(member => member.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(member => member.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(member => member.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(member => member.InvitedAtUtc)
            .HasColumnName("invited_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(member => member.JoinedAtUtc)
            .HasColumnName("joined_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(member => member.RemovedAtUtc)
            .HasColumnName("removed_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(member => member.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Ignore(member => member.DomainEvents);
    }
}
