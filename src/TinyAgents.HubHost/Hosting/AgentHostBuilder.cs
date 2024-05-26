using Serilog;
using Serilog.Events;
using TinyAgents.HubHost.Agents;

namespace TinyAgents.HubHost.Hosting;

internal static class AgentHostBuilder
{
    public static IHost Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseLogging();

        builder.Services.AddSignalR();
        builder.Services.AddApplication(builder.Configuration);

        var app = builder.Build();
        app.MapHub<AgentHub>("/agent");

        return app;
    }

    private static IHostBuilder UseLogging(this IHostBuilder builder)
    {
        // Fallback before configuration can be loaded
        var logger = new LoggerConfiguration()
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3}] {Username} {Message:lj}{NewLine}{Exception}")
            .CreateBootstrapLogger();

        builder.UseSerilog((hostContext, provider, configuration) =>
        {
            if (hostContext.Configuration.GetSection(nameof(Serilog)).Exists())
            {
                logger.Write(LogEventLevel.Information, $"Load {nameof(Serilog)} configurations");
                configuration.ReadFrom.Configuration(hostContext.Configuration);
            }
            else
            {
                logger.Write(LogEventLevel.Warning, $"{nameof(Serilog)} configurations do not exist");
            }
        });

        return builder;
    }
}