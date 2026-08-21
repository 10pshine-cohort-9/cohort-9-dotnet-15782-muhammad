namespace TaskManagementTool.Application.DTOs.Dashboard;

public class DashboardResponse
{
    public int TotalTasks { get; set; }
    public int ToDoCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
}
