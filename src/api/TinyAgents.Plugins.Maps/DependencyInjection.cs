using Azure;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Maps.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TinyAgents.Shared.Http;

namespace TinyAgents.Plugins.Maps;

public static class DependencyInjection
{
    public static IServiceCollection AddMapPlugin(this IServiceCollection services)
    {
        services.AddOptions<MapOptions>()
            .BindConfiguration(nameof(MapOptions))
            .ValidateDataAnnotations();

        var builder = services.AddHttpClient(nameof(MapPlugin));
#if DEBUG
        services.AddTransient<TraceHttpHandler>();
        builder.AddHttpMessageHandler<TraceHttpHandler>();
#endif        
        
        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(MapPlugin));

            var mapOptions = provider.GetRequiredService<IOptions<MapOptions>>().Value;
            if (!string.IsNullOrEmpty(mapOptions.ApiKey))
                return new MapsSearchClient(
                    new AzureKeyCredential(mapOptions.ApiKey),
                    new MapsSearchClientOptions
                    {
                        Transport = new HttpClientTransport(httpClient)
                    });

            if (!string.IsNullOrEmpty(mapOptions.ClientId))
                return new MapsSearchClient(
                    new DefaultAzureCredential(),
                    mapOptions.ClientId,
                    new MapsSearchClientOptions
                    {
                        Transport = new HttpClientTransport(httpClient)
                    });

            throw new InvalidOperationException(
                $"{nameof(MapsSearchClient)} requires either api key or client id credential.");
        });

        services.AddTransient<MapPlugin>();

        return services;
    }
}