using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagementTool.Api.Controller;
using TaskManagementTool.Application.DTOs.Tasks;
using TaskManagementTool.Application.Interfaces;
using Xunit;

namespace TaskManagementTool.Tests;

public class TaskControllerTests
{
    private const int TestUserId = 2;
    private const string TestUserRole = "User";

    private static TasksController CreateController(Mock<ITaskService> mockTaskService, int userId = TestUserId, string role = TestUserRole)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        var controller = new TasksController(mockTaskService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            }
        };

        return controller;
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtActionWithTaskResponse()
    {
        var mockTaskService = new Mock<ITaskService>();
        var request = new CreateTaskRequest { Title = "New Task", PriorityId = 1 };
        var expectedResponse = new TaskResponse { Id = 5, Title = "New Task" };

        mockTaskService
            .Setup(s => s.CreateAsync(request, TestUserId, TestUserRole))
            .ReturnsAsync(expectedResponse);

        var controller = CreateController(mockTaskService);

        var result = await controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<TaskResponse>(createdResult.Value);
        Assert.Equal(5, response.Id);
        Assert.Equal(nameof(TasksController.GetById), createdResult.ActionName);
    }

    [Fact]
    public async Task GetById_ValidRequest_ReturnsOkWithTaskResponse()
    {
        var mockTaskService = new Mock<ITaskService>();
        var expectedResponse = new TaskResponse { Id = 5, Title = "Existing Task" };

        mockTaskService
            .Setup(s => s.GetByIdAsync(5, TestUserId, TestUserRole))
            .ReturnsAsync(expectedResponse);

        var controller = CreateController(mockTaskService);

        var result = await controller.GetById(5);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TaskResponse>(okResult.Value);
        Assert.Equal("Existing Task", response.Title);
    }

    [Fact]
    public async Task GetAll_ValidRequest_ReturnsOkWithTaskList()
    {
        var mockTaskService = new Mock<ITaskService>();
        var expectedList = new List<TaskResponse>
        {
            new() { Id = 1, Title = "Task 1" },
            new() { Id = 2, Title = "Task 2" }
        };

        mockTaskService
            .Setup(s => s.GetAllAsync(TestUserId, TestUserRole, null))
            .ReturnsAsync(expectedList);

        var controller = CreateController(mockTaskService);

        var result = await controller.GetAll(search: null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<List<TaskResponse>>(okResult.Value);
        Assert.Equal(2, response.Count);
    }

    [Fact]
    public async Task GetAll_WithSearchQuery_PassesSearchTermToService()
    {
        var mockTaskService = new Mock<ITaskService>();
        var expectedList = new List<TaskResponse> { new() { Id = 1, Title = "Quarterly report" } };

        mockTaskService
            .Setup(s => s.GetAllAsync(TestUserId, TestUserRole, "report"))
            .ReturnsAsync(expectedList);

        var controller = CreateController(mockTaskService);

        var result = await controller.GetAll(search: "report");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<List<TaskResponse>>(okResult.Value);
        Assert.Single(response);
        mockTaskService.Verify(s => s.GetAllAsync(TestUserId, TestUserRole, "report"), Times.Once);
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsOkWithUpdatedTaskResponse()
    {
        var mockTaskService = new Mock<ITaskService>();
        var request = new UpdateTaskRequest { Title = "Updated Title" };
        var expectedResponse = new TaskResponse { Id = 5, Title = "Updated Title" };

        mockTaskService
            .Setup(s => s.UpdateAsync(5, request, TestUserId, TestUserRole))
            .ReturnsAsync(expectedResponse);

        var controller = CreateController(mockTaskService);

        var result = await controller.Update(5, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TaskResponse>(okResult.Value);
        Assert.Equal("Updated Title", response.Title);
    }

    [Fact]
    public async Task Delete_ValidRequest_ReturnsNoContent()
    {
        var mockTaskService = new Mock<ITaskService>();
        mockTaskService
            .Setup(s => s.DeleteAsync(5, TestUserId, TestUserRole))
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockTaskService);

        var result = await controller.Delete(5);

        Assert.IsType<NoContentResult>(result);
        mockTaskService.Verify(s => s.DeleteAsync(5, TestUserId, TestUserRole), Times.Once);
    }

    [Fact]
    public async Task GetById_AdminClaims_PassesAdminRoleToService()
    {
        //Confirms the controller correctly reads Admin role from claims, not hardcoded
        var mockTaskService = new Mock<ITaskService>();
        var expectedResponse = new TaskResponse { Id = 5, Title = "Task" };

        mockTaskService
            .Setup(s => s.GetByIdAsync(5, 1, "Admin"))
            .ReturnsAsync(expectedResponse);

        var controller = CreateController(mockTaskService, userId: 1, role: "Admin");

        var result = await controller.GetById(5);


        Assert.IsType<OkObjectResult>(result.Result);
        mockTaskService.Verify(s => s.GetByIdAsync(5, 1, "Admin"), Times.Once);
    }
}