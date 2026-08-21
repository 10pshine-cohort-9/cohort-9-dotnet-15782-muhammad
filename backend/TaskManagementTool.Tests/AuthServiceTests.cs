using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManagementTool.Application.DTOs.Auth;
using TaskManagementTool.Application.Exceptions;
using TaskManagementTool.Application.Settings;
using TaskManagementTool.Domain.Entities;
using TaskManagementTool.Infrastructure.Data;
using TaskManagementTool.Infrastructure.Services;
using Xunit;

namespace TaskManagementTool.Tests;

public class AuthServiceTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        context.Roles.Add(new Role { Id = 1, Name = "Admin" });
        context.Roles.Add(new Role { Id = 2, Name = "User" });
        context.SaveChanges();

        return context;
    }

    private static AuthService CreateAuthService(AppDbContext context)
    {
        var jwtSettings = Options.Create(new JwtSettings
        {
            Key = "X7mQ9vLp2Nz8KfRw4HyJc6TaUd1BsE3G", // Testing Key (should be at least 32 characters for HMACSHA256)
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 60
        });

        return new AuthService(context, jwtSettings);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsDuplicateEmailException()
    {
        var context = CreateInMemoryContext();
        var service = CreateAuthService(context);

        context.Users.Add(new User
        {
            FullName = "Existing User",
            Email = "duplicate@example.com",
            PasswordHash = "somehash",
            RoleId = 2,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            FullName = "New User",
            Email = "duplicate@example.com",
            Password = "Valid@123"
        };

        await Assert.ThrowsAsync<DuplicateEmailException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WeakPassword_ThrowsWeakPasswordException()
    {
        var context = CreateInMemoryContext();
        var service = CreateAuthService(context);

        var request = new RegisterRequest
        {
            FullName = "New User",
            Email = "newuser@example.com",
            Password = "weakpassword"
        };

        await Assert.ThrowsAsync<WeakPasswordException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_ValidData_SavesUserWithHashedPasswordAndDefaultRole()
    {
        var context = CreateInMemoryContext();
        var service = CreateAuthService(context);

        var request = new RegisterRequest
        {
            FullName = "New User",
            Email = "newuser@example.com",
            Password = "Valid@123"
        };

        var result = await service.RegisterAsync(request);

        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");

        Assert.NotNull(savedUser);
        Assert.Equal(2, savedUser!.RoleId);
        Assert.NotEqual("Valid@123", savedUser.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Valid@123", savedUser.PasswordHash));
        Assert.Equal("User", result.Role);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task LoginAsync_CorrectCredentials_ReturnsTokenWithCorrectClaims()
    {
        var context = CreateInMemoryContext();
        var service = CreateAuthService(context);

        var registerRequest = new RegisterRequest
        {
            FullName = "Login User",
            Email = "loginuser@example.com",
            Password = "Valid@123"
        };
        await service.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "loginuser@example.com",
            Password = "Valid@123"
        };

        var result = await service.LoginAsync(loginRequest);

        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.Equal("loginuser@example.com", result.Email);
        Assert.Equal("User", result.Role);
        Assert.Equal("Login User", result.FullName);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentialsException()
    {
        var context = CreateInMemoryContext();
        var service = CreateAuthService(context);

        await service.RegisterAsync(new RegisterRequest
        {
            FullName = "Login User",
            Email = "loginuser@example.com",
            Password = "Valid@123"
        });

        var loginRequest = new LoginRequest
        {
            Email = "loginuser@example.com",
            Password = "WrongPassword@1"
        };

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => service.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task LoginAsync_NonExistentEmail_ThrowsSameInvalidCredentialsException()
    {
        var context = CreateInMemoryContext();
        var service = CreateAuthService(context);

        var loginRequest = new LoginRequest
        {
            Email = "doesnotexist@example.com",
            Password = "Whatever@123"
        };

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => service.LoginAsync(loginRequest));
    }

    [Fact]
    public void PasswordHashing_CorrectPasswordVerifiesTrue_WrongPasswordVerifiesFalse()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Valid@123");

        Assert.True(BCrypt.Net.BCrypt.Verify("Valid@123", hash));
        Assert.False(BCrypt.Net.BCrypt.Verify("SomethingElse@1", hash));
    }
}