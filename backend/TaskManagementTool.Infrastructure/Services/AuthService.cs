using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskManagementTool.Application.DTOs.Auth;
using TaskManagementTool.Application.Exceptions;
using TaskManagementTool.Application.Interfaces;
using TaskManagementTool.Application.Settings;
using TaskManagementTool.Domain.Entities;
using TaskManagementTool.Infrastructure.Data;

namespace TaskManagementTool.Infrastructure.Services;

public class AuthService : IAuthService
{
    private const int DefaultUserRoleId = 2; // matches "User" in SeedReferenceData migration

    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            throw new DuplicateEmailException(request.Email);
        }

        if (!IsPasswordStrong(request.Password))
        {
            throw new WeakPasswordException();
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = DefaultUserRoleId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Reload with Role included so token generation has the role name
        var savedUser = await _context.Users
            .Include(u => u.Role)
            .FirstAsync(u => u.Id == user.Id);

        return GenerateAuthResponse(savedUser);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        return GenerateAuthResponse(user);
    }

    private static bool IsPasswordStrong(string password)
    {
        var hasUppercase = Regex.IsMatch(password, "[A-Z]");
        var hasLowercase = Regex.IsMatch(password, "[a-z]");
        var hasSpecialChar = Regex.IsMatch(password, @"[^a-zA-Z0-9]");

        return hasUppercase && hasLowercase && hasSpecialChar;
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponse
        {
            Token = tokenString,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.Name,
            ExpiresAt = expiresAt
        };
    }
}