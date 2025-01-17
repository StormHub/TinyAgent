using Serilog;
using Serilog.Events;
using TinyAgents.SemanticKernel;
using TinyAgents.Shared.Json;

namespace TinyAgents.HubHost.Hosting;

internal static class HubHostBuilder
{
    public static IHost Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseLogging();
        
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddCors(
                options => options.AddPolicy("AllowCors",
                    policyBuilder =>
                    {
                        policyBuilder
                            .WithOrigins("http://localhost:3000")
                            .AllowCredentials()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    })
            );
        }

        builder.Services.AddSignalR()
            .AddJsonProtocol(options => { options.PayloadSerializerOptions.Setup(); });
        builder.Services.AddAgents(builder.Environment);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseCors("AllowCors");
        }
        
        app.MapHub<AgentHub>("/agent");
        app.MapHub<AssistantHub>("/assistant");

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

        builder.UseSerilog((hostContext, _, configuration) =>
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