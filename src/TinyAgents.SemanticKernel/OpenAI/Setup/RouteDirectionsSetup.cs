using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.OpenAI.Plugins;

namespace TinyAgents.SemanticKernel.OpenAI.Setup;

internal sealed class RouteDirectionsSetup(IServiceProvider provider) : IAgentSetup
{
    public string Name => "RouteDirectionsAssistant";

    public string Instructions =>
        """
        You are an assistant helping users to find driving route directions from origin to destination address in Australia.
        The goal is to find the closest routes for users.
        You're laser focused on the goal at hand.
        Answer questions only from given facts.
        Don't waste time with chit chat.
        """;

    public Kernel Configure(Kernel kernel)
    {
        kernel.Plugins.AddFromObject(provider.GetRequiredService<MapPlugin>());
        kernel.Plugins.AddFromObject(provider.GetRequiredService<RoutingPlugin>());

        return kernel;
    }
}