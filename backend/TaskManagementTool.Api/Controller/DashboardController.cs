using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementTool.Application.DTOs.Dashboard;
using TaskManagementTool.Application.Interfaces;

namespace TaskManagementTool.Api.Controller;

[ApiController]
[Route("api/dashboard")]
[Authorize] // every action requires a logged-in user; role-based scoping happens in the service
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        ArgumentNullException.ThrowIfNull(dashboardService);
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> GetDashboard()
    {
        var (userId, role) = GetCurrentUser();
        var result = await _dashboardService.GetDashboardAsync(userId, role);
        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserSummaryResponse>>> GetUserSummaries()
    {
        var (userId, role) = GetCurrentUser();
        var result = await _dashboardService.GetUserSummariesAsync(userId, role);
        return Ok(result);
    }

    private (int UserId, string Role) GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (!int.TryParse(userIdClaim, out var userId) || string.IsNullOrEmpty(role))
        {
            // Should be unreachable behind [Authorize] with a validly-issued token,
            // but fail loudly rather than silently proceeding with userId = 0.
            throw new UnauthorizedAccessException("Token is missing required claims.");
        }

        return (userId, role);
    }
}
