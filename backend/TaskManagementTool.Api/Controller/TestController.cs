using Microsoft.AspNetCore.Mvc;

namespace TaskManagementTool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("throw-error")]
    public IActionResult ThrowTestError()
    {
        throw new InvalidOperationException("This is a deliberate test exception for validation.");
    }
}