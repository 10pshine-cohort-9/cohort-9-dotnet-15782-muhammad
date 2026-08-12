using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagementTool.Application.DTOs.Tasks;
using TaskManagementTool.Application.Exceptions;
using TaskManagementTool.Domain.Entities;
using TaskManagementTool.Infrastructure.Data;
using TaskManagementTool.Infrastructure.Services;
using Xunit;

namespace TaskManagementTool.Tests;

public class TaskServiceTests
{
    private const string AdminRole = "Admin";
    private const string UserRole = "User";

    private const int AdminId = 1;
    private const int UserAId = 2;
    private const int UserBId = 3;

    private const int StatusToDoId = 1;
    private const int StatusInProgressId = 2;

    private const int PriorityLowId = 1;

    private const int CategoryWorkId = 1;
    private const int CategoryOtherId = 4;

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
        context.Statuses.Add(new Status { Id = 3, Name = "Completed" });

        context.Priorities.Add(new Priority { Id = PriorityLowId, Name = "Low" });
        context.Priorities.Add(new Priority { Id = 2, Name = "Medium" });
        context.Priorities.Add(new Priority { Id = 3, Name = "High" });

        context.Categories.Add(new Category { Id = CategoryWorkId, Name = "Work" });
        context.Categories.Add(new Category { Id = 2, Name = "Personal" });
        context.Categories.Add(new Category { Id = 3, Name = "Urgent" });
        context.Categories.Add(new Category { Id = CategoryOtherId, Name = "Other" });

        context.Users.Add(new User { Id = AdminId, FullName = "Admin User", Email = "admin@test.com", PasswordHash = "x", RoleId = 1, CreatedAt = DateTime.UtcNow });
        context.Users.Add(new User { Id = UserAId, FullName = "User A", Email = "usera@test.com", PasswordHash = "x", RoleId = 2, CreatedAt = DateTime.UtcNow });
        context.Users.Add(new User { Id = UserBId, FullName = "User B", Email = "userb@test.com", PasswordHash = "x", RoleId = 2, CreatedAt = DateTime.UtcNow });

        context.SaveChanges();

        return context;
    }

    private static TaskService CreateTaskService(AppDbContext context)
        => new(context, NullLogger<TaskService>.Instance);

    // Helper: inserts a task directly via context, bypassing the service, for Arrange steps.
    private static async Task<int> SeedTaskAsync(AppDbContext context, int createdByUserId, int assignedToUserId, string title = "Seeded Task")
    {
        var task = new TaskItem
        {
            Title = title,
            StatusId = StatusToDoId,
            PriorityId = PriorityLowId,
            CategoryId = CategoryWorkId,
            CreatedByUserId = createdByUserId,
            AssignedToUserId = assignedToUserId,
            CreatedAt = DateTime.UtcNow
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
        return task.Id;
    }

    #region Authorization Scoping

    [Fact]
    public async Task GetByIdAsync_OwnerAsCreator_ReturnsTask()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId);

        var result = await service.GetByIdAsync(taskId, UserAId, UserRole);

        Assert.Equal(taskId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_OwnerAsAssigneeOnly_ReturnsTask()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        // Admin created it, assigned to User A — User A didn't create it but is the assignee
        var taskId = await SeedTaskAsync(context, createdByUserId: AdminId, assignedToUserId: UserAId);

        var result = await service.GetByIdAsync(taskId, UserAId, UserRole);

        Assert.Equal(taskId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NotOwner_ThrowsTaskAccessDeniedException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId);

        await Assert.ThrowsAsync<TaskAccessDeniedException>(() => service.GetByIdAsync(taskId, UserBId, UserRole));
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ThrowsTaskAccessDeniedException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId);

        var request = new UpdateTaskRequest { Title = "hijacked" };

        await Assert.ThrowsAsync<TaskAccessDeniedException>(() => service.UpdateAsync(taskId, request, UserBId, UserRole));
    }

    [Fact]
    public async Task DeleteAsync_NotOwner_ThrowsTaskAccessDeniedException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId);

        await Assert.ThrowsAsync<TaskAccessDeniedException>(() => service.DeleteAsync(taskId, UserBId, UserRole));
    }

    [Fact]
    public async Task GetByIdAsync_AdminNotOwner_ReturnsTask()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId);

        var result = await service.GetByIdAsync(taskId, AdminId, AdminRole);

        Assert.Equal(taskId, result.Id);
    }

    [Fact]
    public async Task GetAllAsync_RegularUser_ReturnsOnlyOwnTasks()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId, title: "A's task");
        await SeedTaskAsync(context, createdByUserId: UserBId, assignedToUserId: UserBId, title: "B's task");

        var result = await service.GetAllAsync(UserAId, UserRole);

        Assert.Single(result);
        Assert.Equal("A's task", result[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_Admin_ReturnsAllTasks()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId, title: "A's task");
        await SeedTaskAsync(context, createdByUserId: UserBId, assignedToUserId: UserBId, title: "B's task");

        var result = await service.GetAllAsync(AdminId, AdminRole);

        Assert.Equal(2, result.Count);
    }

    #endregion

    #region Not Found

    [Fact]
    public async Task GetByIdAsync_NonExistentTaskId_ThrowsTaskNotFoundException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);

        await Assert.ThrowsAsync<TaskNotFoundException>(() => service.GetByIdAsync(9999, UserAId, UserRole));
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedTask_ThrowsTaskNotFoundException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId);
        await service.DeleteAsync(taskId, UserAId, UserRole);

        await Assert.ThrowsAsync<TaskNotFoundException>(() => service.GetByIdAsync(taskId, UserAId, UserRole));
    }

    #endregion

    #region Create - Validation & Defaults

    [Fact]
    public async Task CreateAsync_BlankTitle_ThrowsInvalidTaskReferenceException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var request = new CreateTaskRequest { Title = "   ", PriorityId = PriorityLowId };

        await Assert.ThrowsAsync<InvalidTaskReferenceException>(() => service.CreateAsync(request, UserAId, UserRole));
    }

    [Fact]
    public async Task CreateAsync_InvalidPriorityId_ThrowsInvalidTaskReferenceException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var request = new CreateTaskRequest { Title = "Task", PriorityId = 9999 };

        await Assert.ThrowsAsync<InvalidTaskReferenceException>(() => service.CreateAsync(request, UserAId, UserRole));
    }

    [Fact]
    public async Task CreateAsync_InvalidStatusId_ThrowsInvalidTaskReferenceException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var request = new CreateTaskRequest { Title = "Task", PriorityId = PriorityLowId, StatusId = 9999 };

        await Assert.ThrowsAsync<InvalidTaskReferenceException>(() => service.CreateAsync(request, UserAId, UserRole));
    }

    [Fact]
    public async Task CreateAsync_InvalidCategoryId_ThrowsInvalidTaskReferenceException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var request = new CreateTaskRequest { Title = "Task", PriorityId = PriorityLowId, CategoryId = 9999 };

        await Assert.ThrowsAsync<InvalidTaskReferenceException>(() => service.CreateAsync(request, UserAId, UserRole));
    }

    [Fact]
    public async Task CreateAsync_InvalidAssignedToUserId_ThrowsInvalidTaskReferenceException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var request = new CreateTaskRequest { Title = "Task", PriorityId = PriorityLowId, AssignedToUserId = 9999 };

        // Admin bypasses the self-only restriction, so this hits the "does the user exist" check
        await Assert.ThrowsAsync<InvalidTaskReferenceException>(() => service.CreateAsync(request, AdminId, AdminRole));
    }

    [Fact]
    public async Task CreateAsync_OmittedStatusId_DefaultsToToDo()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var request = new CreateTaskRequest { Title = "Task", PriorityId = PriorityLowId };

        var result = await service.CreateAsync(request, UserAId, UserRole);

        Assert.Equal("To Do", result.StatusName);
    }

    [Fact]
    public async Task CreateAsync_OmittedCategoryId_DefaultsToOther()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var request = new CreateTaskRequest { Title = "Task", PriorityId = PriorityLowId };

        var result = await service.CreateAsync(request, UserAId, UserRole);

        Assert.Equal("Other", result.CategoryName);
    }

    [Fact]
    public async Task CreateAsync_OmittedAssignedToUserId_DefaultsToSelf()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var request = new CreateTaskRequest { Title = "Task", PriorityId = PriorityLowId };

        var result = await service.CreateAsync(request, UserAId, UserRole);

        Assert.Equal(UserAId, result.AssignedToUserId);
    }

    [Fact]
    public async Task CreateAsync_RegularUserAssignsToOther_ThrowsTaskAccessDeniedException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var request = new CreateTaskRequest { Title = "Task", PriorityId = PriorityLowId, AssignedToUserId = UserBId };

        await Assert.ThrowsAsync<TaskAccessDeniedException>(() => service.CreateAsync(request, UserAId, UserRole));
    }

    [Fact]
    public async Task CreateAsync_AdminAssignsToOther_Succeeds()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var request = new CreateTaskRequest { Title = "Task", PriorityId = PriorityLowId, AssignedToUserId = UserBId };

        var result = await service.CreateAsync(request, AdminId, AdminRole);

        Assert.Equal(UserBId, result.AssignedToUserId);
        Assert.True(result.IsAssignedByAdmin);
    }

    #endregion

    #region Update - Partial Update Behavior

    [Fact]
    public async Task UpdateAsync_OnlyStatusIdProvided_OnlyStatusChanges()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId, title: "Original Title");

        var request = new UpdateTaskRequest { StatusId = StatusInProgressId };
        var result = await service.UpdateAsync(taskId, request, UserAId, UserRole);

        Assert.Equal("In Progress", result.StatusName);
        Assert.Equal("Original Title", result.Title); // untouched
        Assert.Equal("Low", result.PriorityName);      // untouched
    }

    [Fact]
    public async Task UpdateAsync_BlankTitle_ThrowsInvalidTaskReferenceException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId);

        var request = new UpdateTaskRequest { Title = "   " };

        await Assert.ThrowsAsync<InvalidTaskReferenceException>(() => service.UpdateAsync(taskId, request, UserAId, UserRole));
    }

    [Fact]
    public async Task UpdateAsync_InvalidPriorityId_ThrowsInvalidTaskReferenceException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId);

        var request = new UpdateTaskRequest { PriorityId = 9999 };

        await Assert.ThrowsAsync<InvalidTaskReferenceException>(() => service.UpdateAsync(taskId, request, UserAId, UserRole));
    }

    [Fact]
    public async Task UpdateAsync_RegularUserReassignsToOther_ThrowsTaskAccessDeniedException()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId);

        var request = new UpdateTaskRequest { AssignedToUserId = UserBId };

        await Assert.ThrowsAsync<TaskAccessDeniedException>(() => service.UpdateAsync(taskId, request, UserAId, UserRole));
    }

    [Fact]
    public async Task UpdateAsync_ValidUpdate_SetsUpdatedAt()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId);

        var request = new UpdateTaskRequest { StatusId = StatusInProgressId };
        var result = await service.UpdateAsync(taskId, request, UserAId, UserRole);

        Assert.NotNull(result.UpdatedAt);
    }

    #endregion

    #region Soft Delete

    [Fact]
    public async Task DeleteAsync_ValidTask_SetsIsDeletedTrueWithoutRemovingRow()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId);

        await service.DeleteAsync(taskId, UserAId, UserRole);

        var taskInDb = await context.Tasks.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == taskId);
        Assert.NotNull(taskInDb);
        Assert.True(taskInDb!.IsDeleted);
    }

    [Fact]
    public async Task GetAllAsync_AfterDelete_ExcludesDeletedTask()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        var taskId = await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId);
        await service.DeleteAsync(taskId, UserAId, UserRole);

        var result = await service.GetAllAsync(UserAId, UserRole);

        Assert.DoesNotContain(result, t => t.Id == taskId);
    }

    #endregion

    #region Search

    [Fact]
    public async Task GetAllAsync_SearchMatchingTitle_ReturnsMatchingTasks()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId, title: "Write quarterly report");
        await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId, title: "Fix bug");

        var result = await service.GetAllAsync(UserAId, UserRole, searchTitle: "report");

        Assert.Single(result);
        Assert.Equal("Write quarterly report", result[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_SearchNoMatch_ReturnsEmptyList()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId, title: "Write quarterly report");

        var result = await service.GetAllAsync(UserAId, UserRole, searchTitle: "zzznomatch");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_SearchStillScopedToOwnTasks()
    {
        var context = CreateInMemoryContext();
        var service = CreateTaskService(context);
        await SeedTaskAsync(context, createdByUserId: UserAId, assignedToUserId: UserAId, title: "Shared keyword task");
        await SeedTaskAsync(context, createdByUserId: UserBId, assignedToUserId: UserBId, title: "Shared keyword task");

        var result = await service.GetAllAsync(UserAId, UserRole, searchTitle: "Shared");

        Assert.Single(result); // only User A's, even though User B's title also matches
    }

    #endregion
}