using Azure;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Maps.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using TinyAgents.Plugins.Maps;
using TinyAgents.Plugins.Search;

namespace TinyAgents.Plugins;

public static class DependencyInjection
{
    public static IServiceCollection AddMapPlugin(this IServiceCollection services)
    {
        services.AddOptions<MapOptions>()
            .BindConfiguration(nameof(MapOptions))
            .ValidateDataAnnotations();

        var builder = services.AddHttpClient(nameof(MapsSearchClient));
        builder.AddStandardResilienceHandler();

        services.AddTransient(provider =>
        {
            var mapOptions = provider.GetRequiredService<IOptions<MapOptions>>().Value;
            if (string.IsNullOrEmpty(mapOptions.ApiKey)
                && string.IsNullOrEmpty(mapOptions.ClientId))
            {
                throw new InvalidOperationException(
                    $"{nameof(MapsSearchClient)} requires either api key or client id credential.");
            }

            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(MapsSearchClient));

            if (!string.IsNullOrEmpty(mapOptions.ApiKey))
                return new MapsSearchClient(
                    new AzureKeyCredential(mapOptions.ApiKey),
                    new MapsSearchClientOptions
                    {
                        Transport = new HttpClientTransport(httpClient)
                    });

            return new MapsSearchClient(
                new DefaultAzureCredential(),
                mapOptions.ClientId,
                new MapsSearchClientOptions
                {
                    Transport = new HttpClientTransport(httpClient)
                });

        });

        services.AddTransient<MapPlugin>();

        return services;
    }

    public static IServiceCollection AddSearchPlugin(this IServiceCollection services)
    {
        services.AddOptions<SearchOptions>()
            .BindConfiguration(nameof(SearchOptions))
            .ValidateDataAnnotations();

        var builder = services.AddHttpClient(nameof(BingConnector));
        builder.AddStandardResilienceHandler();
        
        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(BingConnector));

            var searchOptions = provider.GetRequiredService<IOptions<SearchOptions>>().Value;
            var bingConnector = new BingConnector(
                searchOptions.APIKey, 
                httpClient, 
                loggerFactory: provider.GetRequiredService<ILoggerFactory>());
            
            return new WebSearchEnginePlugin(bingConnector);
        });

        return services;
    }
}