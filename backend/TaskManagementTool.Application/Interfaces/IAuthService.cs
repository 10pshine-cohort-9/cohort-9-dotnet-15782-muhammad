using TaskManagementTool.Application.DTOs.Auth;

namespace TaskManagementTool.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}