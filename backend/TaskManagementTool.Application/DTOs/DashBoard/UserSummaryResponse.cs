namespace TaskManagementTool.Application.DTOs.Dashboard;

public class UserSummaryResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TaskCount { get; set; }
}