using System.Text.Json;
using Azure;
using Azure.Core.Pipeline;
using Azure.Core.Serialization;
using Azure.Identity;
using Azure.Maps.Routing;
using Azure.Maps.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using TinyAgents.Plugins.Maps;
using TinyAgents.Plugins.Search;
using TinyAgents.Shared.Http;

namespace TinyAgents.Plugins;

public static class DependencyInjection
{
    public static IServiceCollection AddMapPlugin(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddOptions<MapOptions>()
            .BindConfiguration(nameof(MapOptions))
            .ValidateDataAnnotations();

        if (environment.IsDevelopment())
        {
            services.AddTransient<TraceHttpHandler>();
        }
        
        var builder = services.AddHttpClient(nameof(MapsSearchClient));
        if (environment.IsProduction())
        {
            builder.AddStandardResilienceHandler();
        }
        if (environment.IsDevelopment())
        {
            builder.AddHttpMessageHandler<TraceHttpHandler>();
        }

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
        services.AddTransient<LocationPlugin>();

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
            var clientOptions = new MapsRoutingClientOptions
            {
                Transport = new HttpClientTransport(httpClient)
            };

            if (!string.IsNullOrEmpty(mapOptions.ApiKey))
                return new MapsRoutingClient(
                    new AzureKeyCredential(mapOptions.ApiKey),
                    clientOptions);

            return new MapsRoutingClient(
                new DefaultAzureCredential(),
                mapOptions.ClientId,
                clientOptions);
        });
        services.AddTransient<RoutingPlugin>();
        
        return services;
    }

    public static IServiceCollection AddSearchPlugin(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddOptions<SearchOptions>()
            .BindConfiguration(nameof(SearchOptions))
            .ValidateDataAnnotations();

        if (environment.IsDevelopment())
        {
            services.AddTransient<TraceHttpHandler>();
        }
        
        var builder = services.AddHttpClient(nameof(BingConnector));
        if (environment.IsProduction())
        {
            builder.AddStandardResilienceHandler();
        }
        if (environment.IsDevelopment())
        {
            builder.AddHttpMessageHandler<TraceHttpHandler>();
        }
        
        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(BingConnector));

            var searchOptions = provider.GetRequiredService<IOptions<SearchOptions>>().Value;
            var bingConnector = new BingConnector(
                apiKey: searchOptions.ApiKey, 
                httpClient: httpClient, 
                loggerFactory: provider.GetRequiredService<ILoggerFactory>());
            return new SearchPlugin(bingConnector, provider.GetRequiredService<ILoggerFactory>());
        });

        return services;
    }
}