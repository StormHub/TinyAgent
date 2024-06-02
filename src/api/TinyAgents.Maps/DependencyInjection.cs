using Azure;
using Azure.Core.Pipeline;
using Azure.Maps.Routing;
using Azure.Maps.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TinyAgents.Maps.Azure.Routing;
using TinyAgents.Maps.Azure.Search;
using TinyAgents.Shared.Http;

namespace TinyAgents.Maps;

public static class DependencyInjection
{
    public static IServiceCollection AddMaps(this IServiceCollection services)
    {
        services.AddOptions<MapOptions>()
            .BindConfiguration(nameof(MapOptions))
            .ValidateDataAnnotations();

        services.AddTransient<TraceHttpHandler>();

        services.AddHttpClient(nameof(MapApi))
            .AddHttpMessageHandler<TraceHttpHandler>();
        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(MapApi));

            var mapOptions = provider.GetRequiredService<IOptions<MapOptions>>().Value;

            return new MapsSearchClient(
                new AzureKeyCredential(mapOptions.ApiKey),
                new MapsSearchClientOptions
                {
                    Transport = new HttpClientTransport(httpClient)
                });
        });
        services.AddTransient<IMapApi, MapApi>();

        services.AddHttpClient(nameof(RouteApi))
            .AddHttpMessageHandler<TraceHttpHandler>();

        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(RouteApi));

            var mapOptions = provider.GetRequiredService<IOptions<MapOptions>>().Value;

            var options = new MapsRoutingClientOptions
            {
                Transport = new HttpClientTransport(httpClient)
            };

            return new MapsRoutingClient(
                new AzureKeyCredential(mapOptions.ApiKey),
                options);
        });
        services.AddTransient<IRouteApi, RouteApi>();

        return services;
    }
}