using System.ClientModel.Primitives;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.Data;
using TinyAgents.Plugins;
using TinyAgents.Plugins.Maps;
using TinyAgents.Plugins.Search;
using TinyAgents.SemanticKernel.Agents;
using TinyAgents.Shared.Http;

namespace TinyAgents.SemanticKernel;

public static class DependencyInjection
{
    public static IServiceCollection AddAgents(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddMap(environment);
        services.AddSearch(environment);
        
        services.AddTransient<LocationAgentFactory>();
        services.AddTransient<SearchAgentFactory>();

        return services.AddKernelBuilder(
            (kernelBuilder, provider) =>
            {
                kernelBuilder.Services.AddSingleton(provider.GetRequiredService<LocationPlugin>());
                kernelBuilder.Services.AddSingleton(provider.GetRequiredService<SearchPlugin>());
                kernelBuilder.Services.AddSingleton(provider.GetRequiredService<RoutingPlugin>());
            },
            environment);
    }

    private static IServiceCollection AddKernelBuilder(this IServiceCollection services,
        Action<IKernelBuilder, IServiceProvider> configureKernelBuilder, 
        IHostEnvironment environment)
    {
        services.AddOptions<OpenAIOptions>()
            .BindConfiguration(nameof(OpenAIOptions))
            .ValidateDataAnnotations();

        if (environment.IsDevelopment())
        {
            services.AddTransient<TraceHttpHandler>();
        }
        
        var builder = services.AddHttpClient(nameof(AzureOpenAIClient));
        if (environment.IsProduction())
        {
            builder.AddStandardResilienceHandler();
        }
        if (environment.IsDevelopment())
        {
            builder.AddHttpMessageHandler<TraceHttpHandler>();
        }
        
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
                deploymentName: openAIOptions.Agents.DeploymentName,
                azureOpenAIClient: azureOpenAIClient,
                serviceId: nameof(openAIOptions.Agents).ToLowerInvariant(),
                modelId: openAIOptions.Agents.ModelId);
            
            kernelBuilder.AddAzureOpenAIChatCompletion(
                deploymentName: openAIOptions.Assistants.DeploymentName,
                azureOpenAIClient: azureOpenAIClient,
                serviceId: nameof(openAIOptions.Assistants).ToLowerInvariant(),
                modelId: openAIOptions.Assistants.ModelId);

            kernelBuilder.Services.AddSingleton(OpenAIClientProvider.FromClient(azureOpenAIClient));
            configureKernelBuilder(kernelBuilder, provider);
            kernelBuilder.Services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());

            return kernelBuilder;
        });

        return services;
    }
}