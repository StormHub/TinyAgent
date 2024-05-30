using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.OpenAI.Plugins;

namespace TinyAgents.SemanticKernel.OpenAI;

internal sealed class ChargingLocationsSetup(IServiceProvider serviceProvider) : IAgentSetup
{
    public string Name => "ChargingLocationsAssistant";

    public string Instructions =>
        """
        You are an assistant helping users to find vehicle charging locations from Postal address, postcode, suburbs in Australia.
        The goal is to find the closest vehicle charging locations for users.
        You're laser focused on the goal at hand.
        Answer questions only from given facts.
        Don't waste time with chit chat.
        """;

    public IKernelBuilder Configure(IKernelBuilder builder)
    {
        var mapPlugin = serviceProvider.GetRequiredService<MapPlugin>();
        builder.Plugins.AddFromObject(mapPlugin);

        var searchPlugin = serviceProvider.GetRequiredService<SearchPlugin>();
        builder.Plugins.AddFromObject(searchPlugin);

        return builder;
    }
}