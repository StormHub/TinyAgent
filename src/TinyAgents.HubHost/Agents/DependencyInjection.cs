using Azure;
using Azure.AI.OpenAI;
using Azure.Core.Pipeline;
using Azure.Maps.Search;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;

namespace TinyAgents.HubHost.Agents;

internal static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddOptions<OpenAIOptions>()
            .BindConfiguration(nameof(OpenAIOptions))
            .ValidateDataAnnotations();
        
        services.AddOptions<AssistantOptions>()
            .BindConfiguration(nameof(AssistantOptions))
            .ValidateDataAnnotations();

        services.AddOptions<LocationOptions>()
            .BindConfiguration(nameof(LocationOptions))
            .ValidateDataAnnotations();

        services.AddHttpClient(nameof(OpenAIClient));
        services.AddHttpClient(nameof(AssistantOptions));
        services.AddHttpClient(nameof(MapsSearchClient));

        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(MapsSearchClient));

            var options = provider.GetRequiredService<IOptions<LocationOptions>>().Value;

            var client = new MapsSearchClient(
                new AzureKeyCredential(options.ApiKey),
                new MapsSearchClientOptions
                {
                    Transport = new HttpClientTransport(httpClient)
                });

            return client;
        });

        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();

            var openAIOptions = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;

            var builder = Kernel.CreateBuilder();
            if (!string.IsNullOrEmpty(openAIOptions.ApiKey))
            {
                builder.AddAzureOpenAIChatCompletion(
                    openAIOptions.ModelId,
                    openAIOptions.Uri.ToString(),
                    openAIOptions.ApiKey,
                    openAIOptions.ModelId,
                    openAIOptions.ModelId,
                    httpClient: factory.CreateClient(nameof(OpenAIClient)));
            }
            else
            {
                builder.AddOpenAIChatCompletion(
                    openAIOptions.ModelId,
                    apiKey: openAIOptions.ApiKey,
                    endpoint: openAIOptions.Uri, 
                    httpClient: factory.CreateClient(nameof(OpenAIClient)));
            }

            var assistantOptions = provider.GetRequiredService<IOptions<AssistantOptions>>().Value;

            builder.Services.AddKeyedSingleton(
                nameof(OpenAIAssistantAgent),
                new OpenAIAssistantConfiguration(
                    assistantOptions.ApiKey,
                    assistantOptions.Uri.ToString())
                {
                    HttpClient = factory.CreateClient(nameof(AssistantOptions))
                });

            builder.Services.AddKeyedSingleton(
                nameof(LocationPlugin), 
                provider.GetRequiredService<MapsSearchClient>());

            builder.Services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());
            return builder;
        });

        services.AddTransient(provider =>
        {
            var options = provider.GetRequiredService<IOptions<AssistantOptions>>().Value;
            return new TripAssistant(
                provider.GetRequiredService<IKernelBuilder>(),
                options.ModelId);
        });

        return services;
    }
}