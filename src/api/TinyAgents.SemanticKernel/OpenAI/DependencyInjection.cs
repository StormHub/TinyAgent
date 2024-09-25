using System.ClientModel.Primitives;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using OpenAI;
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

        var builder = services.AddHttpClient(nameof(OpenAIClient));
#if DEBUG
        services.AddTransient<TraceHttpHandler>();
        builder.AddHttpMessageHandler<TraceHttpHandler>();
#endif

        services.AddSingleton<LocationSetup>();

        services.AddTransient<AzureOpenAIClient>(provider =>
        {
            var openAIOptions = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(OpenAIClient));

            var clientOptions = new AzureOpenAIClientOptions
            {
                Transport = new HttpClientPipelineTransport(httpClient)
            };

            var azureOpenAIClient = !string.IsNullOrEmpty(openAIOptions.ApiKey)
                ? new AzureOpenAIClient(
                    openAIOptions.Uri,
                    new AzureKeyCredential(openAIOptions.ApiKey),
                    clientOptions)
                : new AzureOpenAIClient(
                    openAIOptions.Uri,
                    new DefaultAzureCredential(),
                    clientOptions);
            return azureOpenAIClient;
        });

        services.AddTransient(provider =>
        {
            var openAIOptions = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;

            var kernelBuilder = Kernel.CreateBuilder();
            var azureOpenAIClient = provider.GetRequiredService<AzureOpenAIClient>();

            kernelBuilder.AddAzureOpenAIChatCompletion(
                openAIOptions.ModelId,
                azureOpenAIClient,
                openAIOptions.ModelId,
                openAIOptions.ModelId);

            kernelBuilder.Services.AddKeyedSingleton<IAgentSetup>(
                AssistantAgentType.Locations,
                provider.GetRequiredService<LocationSetup>());
            kernelBuilder.Services.AddSingleton(azureOpenAIClient);
            kernelBuilder.Services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());

            return kernelBuilder;
        });

        return services;
    }
}