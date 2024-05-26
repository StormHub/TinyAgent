using Azure;
using Azure.Core.Pipeline;
using Azure.Maps.Search;
using Microsoft.Extensions.Options;

namespace TinyAgents.HubHost.Agents.Locations;

internal static class DependencyInjection
{
    public static IServiceCollection AddLocations(this IServiceCollection services)
    {
        services.AddOptions<LocationOptions>()
            .BindConfiguration(nameof(LocationOptions))
            .ValidateDataAnnotations();

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
        
        return services;
    }
}