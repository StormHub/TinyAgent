using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TinyAgents.Maps;
using TinyAgents.Search;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.OpenAI;

namespace TinyAgents.SemanticKernel;

public static class DependencyInjection
{
    public static IServiceCollection AddAssistanceAgent(this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddMaps();
        services.AddSearch();
        services.AddOpenAI(environment,
            (builder, provider) =>
            {
                builder.ConfigureMapPlugin(provider);
                builder.ConfigureLocationPlugin(provider);
            });
        services.AddAssistant(configuration);

        return services;
    }
}