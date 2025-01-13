using System.ClientModel.Primitives;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using TinyAgents.Plugins.Maps;
using TinyAgents.SemanticKernel.Agents;
using TinyAgents.Shared.Http;

namespace TinyAgents.SemanticKernel;

public static class DependencyInjection
{
    public static IServiceCollection AddAssistanceAgent(this IServiceCollection services)
    {
        services.AddOptions<OpenAIOptions>()
            .BindConfiguration(nameof(OpenAIOptions))
            .ValidateDataAnnotations();

        // services.AddSingleton<IAgentSetup, LocationSetup>();
        services.AddMapPlugin();

        services.AddTransient<TraceHttpHandler>();
        services.AddHttpClient(nameof(AzureOpenAIClient))
            .AddHttpMessageHandler<TraceHttpHandler>();
            // .AddStandardResilienceHandler();
        
        services.AddTransient<AzureOpenAIClient>(provider =>
        {
            var openAIOptions = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(AzureOpenAIClient));

            var clientOptions = new AzureOpenAIClientOptions
            {
                Transport = new HttpClientPipelineTransport(httpClient)
            };

            var azureOpenAIClient = !string.IsNullOrEmpty(openAIOptions.ApiKey)
                ? new AzureOpenAIClient(
                    new Uri(openAIOptions.Endpoint),
                    new AzureKeyCredential(openAIOptions.ApiKey),
                    clientOptions)
                : new AzureOpenAIClient(
                    new Uri(openAIOptions.Endpoint),
                    new DefaultAzureCredential(),
                    clientOptions);
            
            return azureOpenAIClient;
        });
        
        services.AddTransient(provider =>
        {
            var openAIOptions = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            
            var azureOpenAIClient = provider.GetRequiredService<AzureOpenAIClient>();

            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddAzureOpenAIChatCompletion(
                deploymentName: openAIOptions.DeploymentName,
                azureOpenAIClient: azureOpenAIClient,
                serviceId: "azure",
                modelId: openAIOptions.ModelId);

            kernelBuilder.Services.AddSingleton(OpenAIClientProvider.FromClient(azureOpenAIClient));
            kernelBuilder.Services.AddSingleton(provider.GetRequiredService<MapPlugin>());
            kernelBuilder.Services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());

            return kernelBuilder;
        });
        
        services.AddTransient<LocationAgentFactory>();

        return services;
    }
}