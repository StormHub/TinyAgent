using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TinyAgents.Maps;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.OpenAI;

namespace TinyAgents.SemanticKernel;

public static class DependencyInjection
{
    public static IServiceCollection AddAssistanceAgent(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddMaps();
        services.AddOpenAI();
        services.AddAssistant(configuration);

        return services;
    }
}