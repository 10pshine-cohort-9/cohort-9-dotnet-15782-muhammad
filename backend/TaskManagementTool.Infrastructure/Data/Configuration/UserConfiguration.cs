using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementTool.Domain.Entities;

namespace TaskManagementTool.Infrastructure.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // UpdatedAt is nullable - no config needed beyond what the entity already defines

        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new User
            {
                Id = 1,
                FullName = "Admin",
                Email = "admin101@taskmanagertool.com",
                PasswordHash = "$2a$11$CIX09ywHumg69tSqjEeZne4vicwPZiz7/hr00vmEbusIIX0DULMQS",
                RoleId = 1,
                CreatedAt = new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Utc)
            },
                new User
                {
                    Id = 2,
                    FullName = "Ali Ahmed",
                    Email = "aliahmed45@gmail.com",
                    PasswordHash = "$2a$11$ZuCYGlZy6MsD4Uv9oy18deNV97muiXhMEa6QGtheMyRPjsS75nI8C",
                    RoleId = 2,
                    CreatedAt = new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Utc)
                }
        );
    }
}