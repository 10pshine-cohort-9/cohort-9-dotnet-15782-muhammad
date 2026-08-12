using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManagementTool.Api.Middleware;
using TaskManagementTool.Application.Interfaces;
using TaskManagementTool.Application.Settings;
using TaskManagementTool.Infrastructure.Data;
using TaskManagementTool.Infrastructure.Logging;
using TaskManagementTool.Infrastructure.Services;

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

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITaskService, TaskService>();

    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

    if (string.IsNullOrWhiteSpace(jwtSettings.Key) || Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
    {
        throw new InvalidOperationException(
            "Jwt:Key is missing or too short. It must be at least 32 bytes (256 bits) for HMAC-SHA256.");
    }

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    // Serilog's built-in request logging — logs every HTTP request (method, path, status, duration)
    app.UseSerilogRequestLogging();

    // Global exception handling — must be first so it wraps everything below it
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
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