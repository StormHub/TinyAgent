using Azure;
using Azure.Core.Pipeline;
using Azure.Maps.Routing;
using Azure.Maps.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TinyAgents.Maps.Azure.Routing;
using TinyAgents.Maps.Azure.Search;

namespace TinyAgents.Maps;

public static class DependencyInjection
{
    public static IServiceCollection AddMaps(this IServiceCollection services)
    {
        services.AddOptions<MapOptions>()
            .BindConfiguration(nameof(MapOptions))
            .ValidateDataAnnotations();

        services.AddHttpClient(nameof(MapApi));
        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(MapApi));

            var options = provider.GetRequiredService<IOptions<MapOptions>>().Value;

            return new MapsSearchClient(
                new AzureKeyCredential(options.ApiKey),
                new MapsSearchClientOptions
                {
                    Transport = new HttpClientTransport(httpClient)
                });
        });
        services.AddTransient<IMapApi, MapApi>();

        services.AddHttpClient(nameof(RouteApi));
        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(RouteApi));

            var options = provider.GetRequiredService<IOptions<MapOptions>>().Value;

            return new MapsRoutingClient(
                new AzureKeyCredential(options.ApiKey),
                new MapsRoutingClientOptions
                {
                    Transport = new HttpClientTransport(httpClient)
                });
        });
        services.AddTransient<IRouteApi, RouteApi>();

        return services;
    }
}