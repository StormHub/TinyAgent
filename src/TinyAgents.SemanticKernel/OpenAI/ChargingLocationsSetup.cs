using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.OpenAI.Plugins;

namespace TinyAgents.SemanticKernel.OpenAI;

internal sealed class ChargingLocationsSetup(MapPlugin mapPlugin, SearchPlugin searchPlugin) : IAgentSetup
{
    public string Name => "ChargingLocationsAssistant";

    public string Instructions =>
        """
        You are an assistant helping users to find vehicle charging locations from Postal address, postcode, suburbs in Australia.
        The goal is to find the closest vehicle charging locations for users.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        """;

    public IKernelBuilder Configure(IKernelBuilder builder)
    {
        builder.Plugins.AddFromObject(mapPlugin);
        builder.Plugins.AddFromObject(searchPlugin);

        return builder;
    }
}