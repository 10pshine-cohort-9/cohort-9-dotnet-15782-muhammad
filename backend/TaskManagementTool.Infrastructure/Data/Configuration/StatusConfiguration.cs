using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementTool.Domain.Entities;

namespace TaskManagementTool.Infrastructure.Data.Configuration;

public class StatusConfiguration : IEntityTypeConfiguration<Status>
{
    public void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.ToTable("TaskStatuses");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.Name)
            .IsUnique();

        builder.HasData(
            new Status { Id = 1, Name = "To Do" },
            new Status { Id = 2, Name = "In Progress" },
            new Status { Id = 3, Name = "Completed" }
         );
    }
}