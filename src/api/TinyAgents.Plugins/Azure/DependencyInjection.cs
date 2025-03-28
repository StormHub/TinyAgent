using Azure;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Maps.Routing;
using Azure.Maps.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using TinyAgents.Plugins.Azure.Maps;
using TinyAgents.Plugins.Azure.Search;
using TinyAgents.Shared.Http;

namespace TinyAgents.Plugins.Azure;

public static class DependencyInjection
{
    public static IServiceCollection AddMap(this IServiceCollection services, IHostEnvironment environment)
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

    public static IServiceCollection AddSearch(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddOptions<SearchOptions>()
            .BindConfiguration(nameof(SearchOptions))
            .ValidateDataAnnotations();

        if (environment.IsDevelopment())
        {
            services.AddTransient<TraceHttpHandler>();
        }
        
        var builder = services.AddHttpClient(nameof(BingTextSearch));
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
            var httpClient = factory.CreateClient(nameof(BingTextSearch));
            var searchOptions = provider.GetRequiredService<IOptions<SearchOptions>>().Value;
            
            var textSearchOptions = new BingTextSearchOptions
            {
                HttpClient = httpClient,
                LoggerFactory = provider.GetRequiredService<ILoggerFactory>()
            };
            
           return new BingTextSearch(apiKey: searchOptions.ApiKey, textSearchOptions);
        });
        
        services.AddTransient<SearchPlugin>();

        return services;
    }
}