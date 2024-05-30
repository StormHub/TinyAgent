using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TinyAgents.SemanticKernel.Assistants;

internal static class DependencyInjection
{
    public static IServiceCollection AddAssistant(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IAssistantAgentBuilder, AssistantAgentBuilder>();
        return services;
    }
}