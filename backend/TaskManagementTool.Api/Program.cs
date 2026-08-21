using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskManagementTool.Api.Middleware;
using TaskManagementTool.Infrastructure.Data;
using TaskManagementTool.Infrastructure.Logging;

// Bootstrap logger — catches startup errors before the host is fully built
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting up TaskManagementTool API");

    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog, configured from Infrastructure
    builder.Host.UseSerilog((context, services, configuration) =>
        SerilogConfig.ConfigureSerilog(context.Configuration, configuration)); 

    // Add services to the container.
    builder.Services.AddControllers();

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found. Check User Secrets or appsettings configuration.");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    var app = builder.Build();

    // Global exception handling — must be first so it wraps everything below it
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Serilog's built-in request logging — logs every HTTP request (method, path, status, duration)
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TaskManagementTool API terminated unexpectedly during startup");
    throw; // Re-throw the exception to ensure the process exits with a non-zero exit code
}
finally
{
    Log.CloseAndFlush();
}