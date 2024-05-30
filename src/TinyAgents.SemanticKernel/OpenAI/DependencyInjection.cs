using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.Http;
using TinyAgents.SemanticKernel.OpenAI.Plugins;

namespace TinyAgents.SemanticKernel.OpenAI;

internal static class DependencyInjection
{
    public static IServiceCollection AddOpenAI(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddOptions<OpenAIOptions>()
            .BindConfiguration(nameof(OpenAIOptions))
            .ValidateDataAnnotations();

        var builder = services.AddHttpClient(nameof(OpenAIClient));
        if (environment.IsDevelopment())
        {
            services.AddTransient<TraceHttpHandler>();
            builder.AddHttpMessageHandler<TraceHttpHandler>();
        }

        services.AddTransient<MapPlugin>();
        services.AddTransient<SearchPlugin>();
        services.AddKeyedTransient<IAgentSetup, ChargingLocationsSetup>(nameof(ChargingLocationsSetup));

        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(OpenAIClient));

            var openAIOptions = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;

            var kernelBuilder = Kernel.CreateBuilder();
            if (openAIOptions.Uri.Host.EndsWith("openai.azure.com"))
            {
                kernelBuilder.AddAzureOpenAIChatCompletion(
                    openAIOptions.TextGenerationModelId,
                    openAIOptions.Uri.ToString(),
                    openAIOptions.ApiKey,
                    openAIOptions.TextGenerationModelId,
                    openAIOptions.TextGenerationModelId,
                    httpClient);

                kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
                    openAIOptions.TextEmbeddingModelId,
                    openAIOptions.Uri.ToString(),
                    openAIOptions.ApiKey,
                    openAIOptions.TextEmbeddingModelId,
                    openAIOptions.TextEmbeddingModelId,
                    httpClient);
            }
            else
            {
                kernelBuilder.AddOpenAIChatCompletion(
                    openAIOptions.TextGenerationModelId,
                    apiKey: openAIOptions.ApiKey,
                    endpoint: openAIOptions.Uri,
                    httpClient: httpClient);

                kernelBuilder.AddOpenAITextEmbeddingGeneration(
                    openAIOptions.TextGenerationModelId,
                    openAIOptions.ApiKey,
                    default,
                    openAIOptions.TextGenerationModelId,
                    httpClient);
            }

            kernelBuilder.Services.AddKeyedSingleton(nameof(OpenAIClient), httpClient);
            kernelBuilder.Services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());

            return kernelBuilder;
        });

        return services;
    }
}