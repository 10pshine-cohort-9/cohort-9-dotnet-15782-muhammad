using TaskManagementTool.Application.DTOs.Tasks;

namespace TaskManagementTool.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, int currentUserId, string currentUserRole);
    Task<TaskResponse> GetByIdAsync(int taskId, int currentUserId, string currentUserRole);
    Task<List<TaskResponse>> GetAllAsync(int currentUserId, string currentUserRole, string? searchTitle = null);
    Task<TaskResponse> UpdateAsync(int taskId, UpdateTaskRequest request, int currentUserId, string currentUserRole);
    Task DeleteAsync(int taskId, int currentUserId, string currentUserRole);
}