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
                kernelBuilder.AddAzureOpenAIChatCompletion(
                    openAIOptions.ModelId,
                    openAIOptions.Uri.ToString(),
                    openAIOptions.ApiKey,
                    openAIOptions.ModelId,
                    openAIOptions.ModelId,
                    httpClient);
            else
                kernelBuilder.AddOpenAIChatCompletion(
                    openAIOptions.ModelId,
                    apiKey: openAIOptions.ApiKey,
                    endpoint: openAIOptions.Uri,
                    httpClient: httpClient);

            kernelBuilder.Services.AddKeyedSingleton(nameof(OpenAIClient), httpClient);
            kernelBuilder.Services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());

            configureKernel?.Invoke(kernelBuilder, provider);

            return kernelBuilder;
        });

        return services;
    }
}