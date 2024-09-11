using Microsoft.Extensions.DependencyInjection;
using TinyAgents.Plugins.Maps;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.OpenAI;

namespace TinyAgents.SemanticKernel;

public static class DependencyInjection
{
    public static IServiceCollection AddAssistanceAgent(this IServiceCollection services) => 
        services
            .AddOpenAI()
            .AddAssistant()
            .AddMapPlugin();
}