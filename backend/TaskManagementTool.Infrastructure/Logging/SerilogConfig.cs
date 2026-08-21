using Microsoft.Extensions.Configuration;
using Serilog;

namespace TaskManagementTool.Infrastructure.Logging;

public static class SerilogConfig
{
    public static LoggerConfiguration ConfigureSerilog(
        IConfiguration configuration,
        LoggerConfiguration loggerConfiguration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(loggerConfiguration);
        return loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Async(a => a.File(
                path: Path.Combine(AppContext.BaseDirectory, "..", "Logs", "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
            )) ;
    }
}   