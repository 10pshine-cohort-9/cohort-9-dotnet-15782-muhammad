using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManagementTool.Application.DTOs.Auth;
using TaskManagementTool.Application.Interfaces;

namespace TaskManagementTool.Api.Controller;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        ArgumentNullException.ThrowIfNull(authService, nameof(authService));
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
        {
            return Forbid();
        }

        return Ok(new
        {
            userId,
            email,
            role
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "You are an Admin. Access granted." });
    }
}