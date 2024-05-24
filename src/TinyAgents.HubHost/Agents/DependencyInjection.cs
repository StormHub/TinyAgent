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

        services.AddOptions<LocationOptions>()
            .BindConfiguration(nameof(LocationOptions))
            .ValidateDataAnnotations();

        services.AddHttpClient(nameof(OpenAIClient));
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
            var httpClient = factory.CreateClient(nameof(OpenAIClient));

            var options = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;

            var builder = Kernel.CreateBuilder();
            if (!string.IsNullOrEmpty(options.ApiKey))
                builder.AddAzureOpenAIChatCompletion(
                    options.ModelId,
                    options.Uri.ToString(),
                    options.ApiKey,
                    options.ModelId,
                    options.ModelId,
                    httpClient);
            else
                builder.AddOpenAIChatCompletion(
                    options.ModelId,
                    apiKey: options.ApiKey,
                    endpoint: options.Uri);

            builder.Services.AddKeyedSingleton(nameof(OpenAIAssistantAgent),
                new OpenAIAssistantConfiguration(options.ApiKey, options.Uri.ToString())
                {
                    HttpClient = httpClient
                });

            builder.Services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());
            builder.Services.AddKeyedSingleton(nameof(LocationPlugin), provider.GetRequiredService<MapsSearchClient>());

            return builder;
        });

        services.AddTransient(provider =>
        {
            var options = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            return new TripAssistant(
                provider.GetRequiredService<IKernelBuilder>(),
                options.ModelId);
        });

        return services;
    }
}