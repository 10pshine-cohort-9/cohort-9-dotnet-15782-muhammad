using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementTool.Domain.Entities;

namespace TaskManagementTool.Infrastructure.Data.Configuration;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.HasData(
            new Category { Id = 1, Name = "Work" },
            new Category { Id = 2, Name = "Personal" },
            new Category { Id = 3, Name = "Urgent" },
            new Category { Id = 4, Name = "Other" }
         );
    }
}