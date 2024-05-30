using Azure;
using Azure.Core.Pipeline;
using Azure.Maps.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TinyAgents.Maps.Azure;

namespace TinyAgents.Maps;

public static class DependencyInjection
{
    public static IServiceCollection AddMaps(this IServiceCollection services)
    {
        services.AddOptions<MapOptions>()
            .BindConfiguration(nameof(MapOptions))
            .ValidateDataAnnotations();

        services.AddHttpClient(nameof(MapsSearchClient));

        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(MapsSearchClient));

            var options = provider.GetRequiredService<IOptions<MapOptions>>().Value;

            var client = new MapsSearchClient(
                new AzureKeyCredential(options.ApiKey),
                new MapsSearchClientOptions
                {
                    Transport = new HttpClientTransport(httpClient)
                });

            return client;
        });
        services.AddTransient<IMapApi, MapApi>();
        return services;
    }
}