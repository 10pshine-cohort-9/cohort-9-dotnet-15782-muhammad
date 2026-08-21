using TaskManagementTool.Application.DTOs.Dashboard;

namespace TaskManagementTool.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> GetDashboardAsync(int currentUserId, string currentUserRole);
    Task<List<UserSummaryResponse>> GetUserSummariesAsync(int currentUserId, string currentUserRole);
}
