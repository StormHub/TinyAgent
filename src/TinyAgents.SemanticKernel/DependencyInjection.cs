using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TinyAgents.Locations;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.OpenAI;

namespace TinyAgents.SemanticKernel;

public static class DependencyInjection
{
    public static IServiceCollection AddAssistanceAgent(this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddLocations();
        services.AddOpenAI(environment,
            (builder, provider) => { builder.Services.AddKeyedLocationPlugin(provider); });
        services.AddAssistant(configuration);

        return services;
    }
}