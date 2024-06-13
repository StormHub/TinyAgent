using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace TinyAgents.Local.Hosting;

internal static class ConsoleHostBuilder
{
    public static IHost Build(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((builderContext, builder) =>
            {
                builder.AddJsonFile("appsettings.json", false);
                builder.AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json", true);

                if (builderContext.HostingEnvironment.IsDevelopment()) builder.AddUserSecrets<Program>();

                builder.AddEnvironmentVariables();
            })
            .ConfigureServices((builderContext, services) => { services.AddApplication(); })
            .UseLogging()
            .UseConsoleLifetime()
            .Build();
    }

    private static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddOptions<AgentOptions>()
            .BindConfiguration(nameof(AgentOptions))
            .ValidateDataAnnotations();

        services.AddTransient<AgentConnection>();

        return services;
    }

    private static IHostBuilder UseLogging(this IHostBuilder builder)
    {
        // Fallback before configuration can be loaded
        var logger = new LoggerConfiguration()
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3}] {Username} {Message:lj}{NewLine}{Exception}")
            .CreateBootstrapLogger();

        builder.UseSerilog((hostContext, _, configuration) =>
        {
            if (hostContext.Configuration.GetSection(nameof(Serilog)).Exists())
            {
                logger.Write(LogEventLevel.Information, "Load from Serilog configuration");
                configuration.ReadFrom.Configuration(hostContext.Configuration);
            }
        });

        return builder;
    }
}