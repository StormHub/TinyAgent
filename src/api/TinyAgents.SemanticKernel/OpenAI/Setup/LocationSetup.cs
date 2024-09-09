using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using TinyAgents.Plugins.Maps;
using TinyAgents.SemanticKernel.Assistants;

namespace TinyAgents.SemanticKernel.OpenAI.Setup;

internal sealed class LocationSetup(IServiceProvider provider) : IAgentSetup
{
    public string Name => "RouteDirectionsAssistant";

    public string Version => "2024-06-17";

    public string Instructions =>
        """
        You are an assistant helping users to find GPS locations from postal address in Australia.
        You're laser focused on the goal at hand.
        Answer questions only from given facts.
        """;

    public Kernel Configure(Kernel kernel)
    {
        kernel.Plugins.AddFromObject(provider.GetRequiredService<MapPlugin>());

        return kernel;
    }
}