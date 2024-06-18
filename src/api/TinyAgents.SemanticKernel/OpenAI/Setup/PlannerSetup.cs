using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.OpenAI.Plugins;

namespace TinyAgents.SemanticKernel.OpenAI.Setup;

internal sealed class PlannerSetup(IServiceProvider provider) : IAgentSetup
{
    public string Name => "PlannerAssistant";

    public string Version => "2024-06-17";

    public string Instructions =>
        """
        You are an assistant helping users to plan electric vehicle diving routes from origin to destination address in Australia with charging stations compatible to user vehicle along the way.
        The goal is to find routes for users where charging stations available to the electric vehicle types along the way.
        You're laser focused on the goal at hand.
        Answer questions only from given facts.
        """;

    public Kernel Configure(Kernel kernel)
    {
        kernel.Plugins.AddFromObject(provider.GetRequiredService<MapPlugin>());
        kernel.Plugins.AddFromObject(provider.GetRequiredService<RoutingPlugin>());
        kernel.Plugins.AddFromObject(provider.GetRequiredService<SearchPlugin>());
        kernel.Plugins.AddFromObject(provider.GetRequiredService<PlannerPlugin>());

        return kernel;
    }
}