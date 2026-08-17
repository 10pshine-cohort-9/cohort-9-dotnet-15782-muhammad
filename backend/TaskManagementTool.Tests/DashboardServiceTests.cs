using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagementTool.Application.Exceptions;
using TaskManagementTool.Domain.Entities;
using TaskManagementTool.Infrastructure.Data;
using TaskManagementTool.Infrastructure.Services;
using Xunit;

namespace TaskManagementTool.Tests;

public class DashboardServiceTests
{
    private const string AdminRole = "Admin";
    private const string UserRole = "User";

    private const int AdminId = 1;
    private const int UserAId = 2;
    private const int UserBId = 3;

    private const int StatusToDoId = 1;
    private const int StatusInProgressId = 2;
    private const int StatusCompletedId = 3;

    private const int PriorityLowId = 1;

    private const int CategoryWorkId = 1;
    private const int CategoryPersonalId = 2;

    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        context.Roles.Add(new Role { Id = 1, Name = AdminRole });
        context.Roles.Add(new Role { Id = 2, Name = UserRole });

        context.Statuses.Add(new Status { Id = StatusToDoId, Name = "To Do" });
        context.Statuses.Add(new Status { Id = StatusInProgressId, Name = "In Progress" });
        context.Statuses.Add(new Status { Id = StatusCompletedId, Name = "Completed" });

        context.Priorities.Add(new Priority { Id = PriorityLowId, Name = "Low" });

        context.Categories.Add(new Category { Id = CategoryWorkId, Name = "Work" });
        context.Categories.Add(new Category { Id = CategoryPersonalId, Name = "Personal" });

        context.Users.Add(new User { Id = AdminId, FullName = "Admin User", Email = "admin@test.com", PasswordHash = "x", RoleId = 1, CreatedAt = DateTime.UtcNow });
        context.Users.Add(new User { Id = UserAId, FullName = "User A", Email = "usera@test.com", PasswordHash = "x", RoleId = 2, CreatedAt = DateTime.UtcNow });
        context.Users.Add(new User { Id = UserBId, FullName = "User B", Email = "userb@test.com", PasswordHash = "x", RoleId = 2, CreatedAt = DateTime.UtcNow });

        context.SaveChanges();

        return context;
    }

    private static DashboardService CreateDashboardService(AppDbContext context)
        => new(context, NullLogger<DashboardService>.Instance);

    private static async Task SeedTaskAsync(AppDbContext context, int createdByUserId, int assignedToUserId, int statusId, int categoryId = CategoryWorkId)
    {
        context.Tasks.Add(new TaskItem
        {
            Title = "Seeded Task",
            StatusId = statusId,
            PriorityId = PriorityLowId,
            CategoryId = categoryId,
            CreatedByUserId = createdByUserId,
            AssignedToUserId = assignedToUserId,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetDashboardAsync_User_CountsOnlyOwnTasks()
    {
        var context = CreateInMemoryContext();
        var service = CreateDashboardService(context);
        await SeedTaskAsync(context, UserAId, UserAId, StatusToDoId);
        await SeedTaskAsync(context, UserBId, UserBId, StatusToDoId); // not User A's

        var result = await service.GetDashboardAsync(UserAId, UserRole);

        Assert.Equal(1, result.TotalTasks);
        Assert.Equal(1, result.ToDoCount);
    }

    [Fact]
    public async Task GetDashboardAsync_Admin_CountsAllTasksAcrossUsers()
    {
        var context = CreateInMemoryContext();
        var service = CreateDashboardService(context);
        await SeedTaskAsync(context, UserAId, UserAId, StatusToDoId);
        await SeedTaskAsync(context, UserBId, UserBId, StatusInProgressId);

        var result = await service.GetDashboardAsync(AdminId, AdminRole);

        Assert.Equal(2, result.TotalTasks);
        Assert.Equal(1, result.ToDoCount);
        Assert.Equal(1, result.InProgressCount);
    }

    [Fact]
    public async Task GetDashboardAsync_Admin_ExcludesOtherUsersPersonalTasks()
    {
        var context = CreateInMemoryContext();
        var service = CreateDashboardService(context);
        await SeedTaskAsync(context, UserAId, UserAId, StatusToDoId, categoryId: CategoryPersonalId);
        await SeedTaskAsync(context, UserAId, UserAId, StatusToDoId, categoryId: CategoryWorkId);

        var result = await service.GetDashboardAsync(AdminId, AdminRole);

        Assert.Equal(1, result.TotalTasks);
    }

    [Fact]
    public async Task GetDashboardAsync_NoTasks_ReturnsAllZeros()
    {
        var context = CreateInMemoryContext();
        var service = CreateDashboardService(context);

        var result = await service.GetDashboardAsync(UserAId, UserRole);

        Assert.Equal(0, result.TotalTasks);
        Assert.Equal(0, result.ToDoCount);
        Assert.Equal(0, result.InProgressCount);
        Assert.Equal(0, result.CompletedCount);
    }

    [Fact]
    public async Task GetUserSummariesAsync_Admin_ReturnsAllUsersWithTaskCounts()
    {
        var context = CreateInMemoryContext();
        var service = CreateDashboardService(context);
        await SeedTaskAsync(context, UserAId, UserAId, StatusToDoId);
        await SeedTaskAsync(context, AdminId, UserAId, StatusInProgressId);

        var result = await service.GetUserSummariesAsync(AdminId, AdminRole);

        var userASummary = result.Single(u => u.UserId == UserAId);
        Assert.Equal(2, userASummary.TaskCount);
    }

    [Fact]
    public async Task GetUserSummariesAsync_NonAdmin_ThrowsTaskAccessDeniedException()
    {
        var context = CreateInMemoryContext();
        var service = CreateDashboardService(context);

        await Assert.ThrowsAsync<TaskAccessDeniedException>(() => service.GetUserSummariesAsync(UserAId, UserRole));
    }
}