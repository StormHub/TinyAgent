using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.OpenAI.Plugins;
using TinyAgents.SemanticKernel.OpenAI.Setup;

namespace TinyAgents.SemanticKernel.OpenAI;

internal static class DependencyInjection
{
    public static IServiceCollection AddOpenAI(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddOptions<OpenAIOptions>()
            .BindConfiguration(nameof(OpenAIOptions))
            .ValidateDataAnnotations();

        // services.AddTransient<TraceHttpHandler>();
        services.AddHttpClient(nameof(OpenAIClient));
            // .AddHttpMessageHandler<TraceHttpHandler>();

        services.AddTransient<MapPlugin>();
        services.AddTransient<SearchPlugin>();
        services.AddTransient<RoutingPlugin>();

        services.AddSingleton<ChargingLocationsSetup>();
        services.AddSingleton<RouteDirectionsSetup>();

        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(OpenAIClient));

            var openAIOptions = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;

            var kernelBuilder = Kernel.CreateBuilder();
            if (openAIOptions.Uri.Host.EndsWith("openai.azure.com"))
                kernelBuilder.AddAzureOpenAIChatCompletion(
                    openAIOptions.TextGenerationModelId,
                    openAIOptions.Uri.ToString(),
                    openAIOptions.ApiKey,
                    openAIOptions.TextGenerationModelId,
                    openAIOptions.TextGenerationModelId,
                    httpClient);
            else
                kernelBuilder.AddOpenAIChatCompletion(
                    openAIOptions.TextGenerationModelId,
                    apiKey: openAIOptions.ApiKey,
                    endpoint: openAIOptions.Uri,
                    orgId: openAIOptions.OrganizationId,
                    httpClient: httpClient);

            kernelBuilder.Services.AddKeyedSingleton<IAgentSetup>(
                AssistantAgentType.ChargingLocations,
                provider.GetRequiredService<ChargingLocationsSetup>());

            kernelBuilder.Services.AddKeyedSingleton<IAgentSetup>(
                AssistantAgentType.RouteDirections,
                provider.GetRequiredService<RouteDirectionsSetup>());

            kernelBuilder.Services.AddKeyedSingleton(nameof(OpenAIClient), httpClient);
            kernelBuilder.Services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());

            return kernelBuilder;
        });

        return services;
    }
}