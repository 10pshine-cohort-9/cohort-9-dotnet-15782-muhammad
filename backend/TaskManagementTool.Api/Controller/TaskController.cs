using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementTool.Application.DTOs.Tasks;
using TaskManagementTool.Application.Interfaces;

namespace TaskManagementTool.Api.Controller;

[ApiController]
[Route("api/tasks")]
[Authorize] // every action requires a logged-in user; role-based scoping happens in the service
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request)
    {
        var (userId, role) = GetCurrentUser();
        var result = await _taskService.CreateAsync(request, userId, role);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id)
    {
        var (userId, role) = GetCurrentUser();
        var result = await _taskService.GetByIdAsync(id, userId, role);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<TaskResponse>>> GetAll([FromQuery] string? search)
    {
        var (userId, role) = GetCurrentUser();
        var result = await _taskService.GetAllAsync(userId, role, search);
        return Ok(result);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<TaskResponse>> Update(int id, UpdateTaskRequest request)
    {
        var (userId, role) = GetCurrentUser();
        var result = await _taskService.UpdateAsync(id, request, userId, role);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (userId, role) = GetCurrentUser();
        await _taskService.DeleteAsync(id, userId, role);
        return NoContent();
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