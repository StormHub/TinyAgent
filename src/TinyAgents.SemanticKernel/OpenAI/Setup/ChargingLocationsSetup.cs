using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.OpenAI.Plugins;

namespace TinyAgents.SemanticKernel.OpenAI.Setup;

internal sealed class ChargingLocationsSetup(IServiceProvider provider) : IAgentSetup
{
    public string Name => "ChargingLocationsAssistant";

    public string Instructions =>
        """
        You are an assistant helping users to find electric vehicle charging locations offer compatible charging connectors from Postal address, postcode, suburbs in Australia.
        The goal is to find the closest vehicle charging locations offers compatible charger types for users.
        Answer questions only from given facts.
        """;

    public Kernel Configure(Kernel kernel)
    {
        kernel.Plugins.AddFromObject(provider.GetRequiredService<MapPlugin>());
        kernel.Plugins.AddFromObject(provider.GetRequiredService<SearchPlugin>());

        return kernel;
    }
}