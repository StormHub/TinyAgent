using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Http;

namespace TinyAgents.SemanticKernel.OpenAI;

internal static class DependencyInjection
{
    public static IServiceCollection AddOpenAI(this IServiceCollection services, IHostEnvironment environment,
        Action<IKernelBuilder, IServiceProvider>? configureKernel = default)
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

        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(OpenAIClient));

            var openAIOptions = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;

            var kernelBuilder = Kernel.CreateBuilder();
            if (openAIOptions.Uri.Host.EndsWith("openai.azure.com"))
            {
                kernelBuilder.AddAzureOpenAIChatCompletion(
                    deploymentName:openAIOptions.TextGenerationModelId,
                    endpoint: openAIOptions.Uri.ToString(),
                    openAIOptions.ApiKey,
                    serviceId: openAIOptions.TextGenerationModelId,
                    openAIOptions.TextGenerationModelId,
                    httpClient);

                kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
                    deploymentName: openAIOptions.TextEmbeddingModelId, 
                    endpoint: openAIOptions.Uri.ToString(),
                    apiKey: openAIOptions.ApiKey,
                    serviceId: openAIOptions.TextEmbeddingModelId,
                    modelId: openAIOptions.TextEmbeddingModelId,
                    httpClient: httpClient);
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
                    apiKey: openAIOptions.ApiKey,
                    orgId: default,
                    serviceId: openAIOptions.TextGenerationModelId,
                    httpClient: httpClient);
            }

            kernelBuilder.Services.AddKeyedSingleton(nameof(OpenAIClient), httpClient);
            kernelBuilder.Services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());

            configureKernel?.Invoke(kernelBuilder, provider);

            return kernelBuilder;
        });

        return services;
    }
}