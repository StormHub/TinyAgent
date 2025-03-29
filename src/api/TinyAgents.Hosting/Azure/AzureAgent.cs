using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TinyAgents.SemanticKernel.AzureAI;
using TinyAgents.SemanticKernel.Json;

namespace TinyAgents.Hosting.Azure;

public record Charger(string Type, string Description);

internal static class AzureAgent
{
    public static void ConfigureServices(HostBuilderContext builderContext, IServiceCollection services)
    {
        services.AddAzureAgents(builderContext.HostingEnvironment);
    }

    public static async Task<IHostApplicationLifetime> RunSearchAgent(this IHost host)
    {
        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var factory = scope.ServiceProvider.GetRequiredService<SearchAgentFactory>();

            var agentProxy = await factory.CreateAgent();
            var input = 
                $"""
                 Question:
                 What are the charger types of tesla model 3?

                 Respond in JSON format with the following JSON schema:
                 {ResponseFormat.JsonSchema<Charger[]>().GetRawText()}
                 """;

            var response = agentProxy.Invoke(input, cancellationToken: lifetime.ApplicationStopping);
            await foreach (var message in response)
            {
                Console.WriteLine(message.Content);
            }
        }

        lifetime.StopApplication();
        return lifetime;
    }
}