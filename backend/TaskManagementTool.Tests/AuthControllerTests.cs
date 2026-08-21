using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagementTool.Api.Controller;
using TaskManagementTool.Application.DTOs.Auth;
using TaskManagementTool.Application.Interfaces;
using Xunit;

namespace TaskManagementTool.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ValidRequest_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var request = new RegisterRequest
        {
            FullName = "New User",
            Email = "newuser@example.com",
            Password = "Valid@123"
        };
        var expectedResponse = new AuthResponse
        {
            Token = "fake-token",
            Email = request.Email,
            FullName = request.FullName,
            Role = "User",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        mockAuthService
            .Setup(s => s.RegisterAsync(request))
            .ReturnsAsync(expectedResponse);

        var controller = new AuthController(mockAuthService.Object);

        // Act
        var result = await controller.Register(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal("newuser@example.com", response.Email);
    }

    [Fact]
    public async Task Login_ValidRequest_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "Valid@123"
        };
        var expectedResponse = new AuthResponse
        {
            Token = "fake-token",
            Email = request.Email,
            FullName = "Existing User",
            Role = "User",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        mockAuthService
            .Setup(s => s.LoginAsync(request))
            .ReturnsAsync(expectedResponse);

        var controller = new AuthController(mockAuthService.Object);

        // Act
        var result = await controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal("user@example.com", response.Email);
    }
}