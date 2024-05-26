using Azure.Maps.Search;
using TinyAgents.HubHost.Agents.Assistants;
using TinyAgents.HubHost.Agents.Locations;
using TinyAgents.HubHost.Agents.OpenAI;

namespace TinyAgents.HubHost.Agents;

internal static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLocations();
        services.AddOpenAI((builder, provider) =>
        {
            builder.Services.AddKeyedSingleton(
                nameof(LocationPlugin), 
                provider.GetRequiredService<MapsSearchClient>());
        });
        services.AddAssistant(configuration);
        
        return services;
    }
}