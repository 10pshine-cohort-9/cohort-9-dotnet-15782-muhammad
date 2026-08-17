using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagementTool.Application.DTOs.Dashboard;
using TaskManagementTool.Application.Interfaces;
using TaskManagementTool.Infrastructure.Data;

namespace TaskManagementTool.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private const string AdminRole = "Admin";
    private const string ToDoStatusName = "To Do";
    private const string InProgressStatusName = "In Progress";
    private const string CompletedStatusName = "Completed";
    private const string PersonalCategoryName = "Personal";

    private readonly AppDbContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(AppDbContext context, ILogger<DashboardService> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _logger = logger;
    }

    public async Task<DashboardResponse> GetDashboardAsync(int currentUserId, string currentUserRole)
    {
        var query = _context.Tasks
            .Include(t => t.Status)
            .Where(t => !t.IsDeleted);

        if (currentUserRole != AdminRole)
        {
            query = query.Where(t => t.CreatedByUserId == currentUserId || t.AssignedToUserId == currentUserId);
        }
        else
        {
            query = query.Where(t => t.Category.Name != PersonalCategoryName || t.CreatedByUserId == currentUserId);
        }

        // Group by status name rather than a hardcoded StatusId - resilient to seed data reordering,
        // matching the lookup-by-Name pattern already used in TaskService for defaults.
        var counts = await query
            .GroupBy(t => t.Status.Name)
            .Select(g => new { StatusName = g.Key, Count = g.Count() })
            .ToListAsync();

        var response = new DashboardResponse
        {
            ToDoCount = counts.FirstOrDefault(c => c.StatusName == ToDoStatusName)?.Count ?? 0,
            InProgressCount = counts.FirstOrDefault(c => c.StatusName == InProgressStatusName)?.Count ?? 0,
            CompletedCount = counts.FirstOrDefault(c => c.StatusName == CompletedStatusName)?.Count ?? 0,
            // Summed across ALL statuses (not just the three named buckets), so a future
            // status added to the seed data still counts toward the total without a code change.
            TotalTasks = counts.Sum(c => c.Count)
        };

        _logger.LogInformation(
            "Dashboard counts requested by user {UserId} (role {Role}): Total={Total}, ToDo={ToDo}, InProgress={InProgress}, Completed={Completed}.",
            currentUserId, currentUserRole, response.TotalTasks, response.ToDoCount, response.InProgressCount, response.CompletedCount);

        return response;
    }
}