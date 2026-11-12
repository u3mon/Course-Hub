using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class AdminActionLogConfiguration : IEntityTypeConfiguration<AdminActionLog>
{
    public void Configure(EntityTypeBuilder<AdminActionLog> builder)
    {
        builder.ToTable("AdminActionLogs");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.AdminEmail).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Action).IsRequired().HasMaxLength(150);
        builder.Property(l => l.EntityName).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Details).IsRequired().HasMaxLength(1000);
    }
}
