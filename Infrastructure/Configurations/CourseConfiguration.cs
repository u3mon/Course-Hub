using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(150);
        builder.Property(c => c.InstructorName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(3000);
        builder.Property(c => c.Duration).IsRequired().HasMaxLength(60);
        builder.Property(c => c.Price).HasPrecision(18, 2);
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
    }
}
