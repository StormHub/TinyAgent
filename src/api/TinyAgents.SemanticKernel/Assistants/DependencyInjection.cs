using Microsoft.Extensions.DependencyInjection;

namespace TinyAgents.SemanticKernel.Assistants;

internal static class DependencyInjection
{
    public static IServiceCollection AddAssistant(this IServiceCollection services)
    {
        services.AddTransient<IAssistantAgentBuilder, AssistantAgentBuilder>();
        return services;
    }
}