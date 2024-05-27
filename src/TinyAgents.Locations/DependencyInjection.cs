using Azure;
using Azure.Core.Pipeline;
using Azure.Maps.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace TinyAgents.Locations;

public static class DependencyInjection
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

    public static IServiceCollection AddKeyedLocationPlugin(this IServiceCollection services, IServiceProvider provider)
    {
        services.AddKeyedSingleton(
            nameof(LocationPlugin), 
            provider.GetRequiredService<MapsSearchClient>());
        return services;
    }

    public static void AddLocationPlugin(this Kernel kernel)
    {
        LocationPlugin.ScopeTo(kernel);
    }
}