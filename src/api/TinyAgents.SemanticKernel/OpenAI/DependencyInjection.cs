using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using OpenAI;
using TinyAgents.Plugins.Maps;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.OpenAI.Setup;
using TinyAgents.Shared.Http;

namespace TinyAgents.SemanticKernel.OpenAI;

internal static class DependencyInjection
{
    public static IServiceCollection AddOpenAI(this IServiceCollection services)
    {
        services.AddOptions<OpenAIOptions>()
            .BindConfiguration(nameof(OpenAIOptions))
            .ValidateDataAnnotations();

        services.AddTransient<TraceHttpHandler>();
        services.AddHttpClient(nameof(OpenAIClient)).AddHttpMessageHandler<TraceHttpHandler>();

        services.AddTransient<MapPlugin>();

        services.AddSingleton<LocationSetup>();

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
                AssistantAgentType.Locations,
                provider.GetRequiredService<LocationSetup>());

            kernelBuilder.Services.AddKeyedSingleton(nameof(OpenAIClient), httpClient);
            kernelBuilder.Services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());

            return kernelBuilder;
        });

        return services;
    }
}