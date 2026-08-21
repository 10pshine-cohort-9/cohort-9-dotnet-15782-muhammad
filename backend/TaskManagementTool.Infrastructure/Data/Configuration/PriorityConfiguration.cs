using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementTool.Domain.Entities;

namespace TaskManagementTool.Infrastructure.Data.Configuration;

public class PriorityConfiguration : IEntityTypeConfiguration<Priority>
{
    public void Configure(EntityTypeBuilder<Priority> builder)
    {
        builder.ToTable("TaskPriorities");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.HasData(
            new Priority { Id = 1, Name = "Low" },
            new Priority { Id = 2, Name = "Medium" },
            new Priority { Id = 3, Name = "High" }
         );
    }
}