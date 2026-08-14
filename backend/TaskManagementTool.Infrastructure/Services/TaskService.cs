using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagementTool.Application.DTOs.Tasks;
using TaskManagementTool.Application.Exceptions;
using TaskManagementTool.Application.Interfaces;
using TaskManagementTool.Domain.Entities;
using TaskManagementTool.Infrastructure.Data;

namespace TaskManagementTool.Infrastructure.Services;

public class TaskService : ITaskService
{
    private const string AdminRole = "Admin";
    private const string DefaultStatusName = "To Do";
    private const string DefaultCategoryName = "Other";

    private readonly AppDbContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(AppDbContext context, ILogger<TaskService> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _logger = logger;
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, int currentUserId, string currentUserRole)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidTaskReferenceException("Title is required.");
        }

        if (request.DueDate.HasValue && request.DueDate.Value == DateTime.MinValue)
        {
            throw new InvalidTaskReferenceException("DueDate is not a valid date.");
        }

        var priority = await _context.Priorities.FindAsync(request.PriorityId)
            ?? throw new InvalidTaskReferenceException($"PriorityId '{request.PriorityId}' does not exist.");

        var statusId = request.StatusId
            ?? (await _context.Statuses.FirstOrDefaultAsync(s => s.Name == DefaultStatusName)
                ?? throw new InvalidOperationException($"Seed status '{DefaultStatusName}' is missing.")).Id;

        var status = await _context.Statuses.FindAsync(statusId)
            ?? throw new InvalidTaskReferenceException($"StatusId '{statusId}' does not exist.");

        var categoryId = request.CategoryId
            ?? (await _context.Categories.FirstOrDefaultAsync(c => c.Name == DefaultCategoryName)
                ?? throw new InvalidOperationException($"Seed category '{DefaultCategoryName}' is missing.")).Id;

        var category = await _context.Categories.FindAsync(categoryId)
            ?? throw new InvalidTaskReferenceException($"CategoryId '{categoryId}' does not exist.");

        var assignedToUserId = request.AssignedToUserId ?? currentUserId;

        if (currentUserRole != AdminRole && assignedToUserId != currentUserId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to assign a task to another user {TargetUserId}.",
                currentUserId, assignedToUserId);
            throw new TaskAccessDeniedException();
        }

        if (currentUserRole == AdminRole && assignedToUserId == currentUserId)
        {
            _logger.LogWarning(
                "Admin {UserId} attempted to self-assign a task, which is not permitted.",
                currentUserId);
            throw new InvalidTaskReferenceException("Admins cannot assign tasks to themselves.");
        }

        var assignedToUser = await _context.Users.FindAsync(assignedToUserId)
            ?? throw new InvalidTaskReferenceException($"AssignedToUserId '{assignedToUserId}' does not exist.");

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            StatusId = status.Id,
            PriorityId = priority.Id,
            CategoryId = category.Id,
            CreatedByUserId = currentUserId,
            AssignedToUserId = assignedToUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Task {TaskId} created by user {UserId}, assigned to {AssignedToUserId}.",
            task.Id, currentUserId, assignedToUserId);

        return await GetByIdInternalAsync(task.Id, currentUserId, currentUserRole);
    }

    public async Task<TaskResponse> GetByIdAsync(int taskId, int currentUserId, string currentUserRole)
        => await GetByIdInternalAsync(taskId, currentUserId, currentUserRole);

    public async Task<List<TaskResponse>> GetAllAsync(int currentUserId, string currentUserRole, string? searchTitle = null)
    {
        var query = _context.Tasks
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Where(t => !t.IsDeleted);

        if (currentUserRole != AdminRole)
        {
            query = query.Where(t => t.CreatedByUserId == currentUserId || t.AssignedToUserId == currentUserId);
        }

        if(!string.IsNullOrWhiteSpace(searchTitle))
        {
            query = query.Where(t => t.Title.Contains(searchTitle));
        }

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToResponse).ToList();
    }

    public async Task<TaskResponse> UpdateAsync(int taskId, UpdateTaskRequest request, int currentUserId, string currentUserRole)
    {
        ArgumentNullException.ThrowIfNull(request);

        var task = await GetOwnedTaskEntityAsync(taskId, currentUserId, currentUserRole);

        if (request.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new InvalidTaskReferenceException("Title cannot be blank.");
            }
            task.Title = request.Title;
        }

        if (request.Description is not null)
        {
            task.Description = request.Description;
        }

        if (request.DueDate is not null)
        {
            if (request.DueDate.Value == DateTime.MinValue)
            {
                throw new InvalidTaskReferenceException("DueDate is not a valid date.");
            }
            task.DueDate = request.DueDate;
        }

        if (request.StatusId is not null)
        {
            _ = await _context.Statuses.FindAsync(request.StatusId.Value)
                ?? throw new InvalidTaskReferenceException($"StatusId '{request.StatusId}' does not exist.");
            task.StatusId = request.StatusId.Value;
        }

        if (request.PriorityId is not null)
        {
            _ = await _context.Priorities.FindAsync(request.PriorityId.Value)
                ?? throw new InvalidTaskReferenceException($"PriorityId '{request.PriorityId}' does not exist.");
            task.PriorityId = request.PriorityId.Value;
        }

        if (request.CategoryId is not null)
        {
            _ = await _context.Categories.FindAsync(request.CategoryId.Value)
                ?? throw new InvalidTaskReferenceException($"CategoryId '{request.CategoryId}' does not exist.");
            task.CategoryId = request.CategoryId.Value;
        }

        if (request.AssignedToUserId is not null)
        {
            if (currentUserRole != AdminRole && request.AssignedToUserId.Value != currentUserId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to reassign task {TaskId} to another user {TargetUserId}.",
                    currentUserId, taskId, request.AssignedToUserId.Value);
                throw new TaskAccessDeniedException();
            }

            if (currentUserRole == AdminRole && request.AssignedToUserId.Value == currentUserId)
            {
                _logger.LogWarning(
                    "Admin {UserId} attempted to self-assign task {TaskId}, which is not permitted.",
                    currentUserId, taskId);
                throw new InvalidTaskReferenceException("Admins cannot assign tasks to themselves.");
            }

            _ = await _context.Users.FindAsync(request.AssignedToUserId.Value)
                ?? throw new InvalidTaskReferenceException($"AssignedToUserId '{request.AssignedToUserId}' does not exist.");
            task.AssignedToUserId = request.AssignedToUserId.Value;
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} updated by user {UserId}.", taskId, currentUserId);

        return await GetByIdInternalAsync(taskId, currentUserId, currentUserRole);
    }

    public async Task DeleteAsync(int taskId, int currentUserId, string currentUserRole)
    {
        var task = await GetOwnedTaskEntityAsync(taskId, currentUserId, currentUserRole);

        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} soft-deleted by user {UserId}.", taskId, currentUserId);
    }

    // Shared read path: loads, checks not-found, checks ownership, maps to response.
    private async Task<TaskResponse> GetByIdInternalAsync(int taskId, int currentUserId, string currentUserRole)
    {
        var task = await GetOwnedTaskEntityAsync(taskId, currentUserId, currentUserRole);
        return MapToResponse(task);
    }

    // Shared write/read path: loads tracked entity, enforces not-found + ownership scoping.
    private async Task<TaskItem> GetOwnedTaskEntityAsync(int taskId, int currentUserId, string currentUserRole)
    {
        var task = await _context.Tasks
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

        if (task is null)
        {
            throw new TaskNotFoundException(taskId);
        }

        var isOwner = task.CreatedByUserId == currentUserId || task.AssignedToUserId == currentUserId;

        if (currentUserRole != AdminRole && !isOwner)
        {
            _logger.LogWarning(
                "User {UserId} attempted unauthorized access to task {TaskId}.",
                currentUserId, taskId);
            throw new TaskAccessDeniedException();
        }

        return task;
    }

    private static TaskResponse MapToResponse(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        DueDate = task.DueDate,
        StatusId = task.StatusId,
        StatusName = task.Status.Name,
        PriorityId = task.PriorityId,
        PriorityName = task.Priority.Name,
        CategoryId = task.CategoryId,
        CategoryName = task.Category.Name,
        CreatedByUserId = task.CreatedByUserId,
        CreatedByUserName = task.CreatedByUser.FullName,
        AssignedToUserId = task.AssignedToUserId,
        AssignedToUserName = task.AssignedToUser.FullName,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt
    };
}