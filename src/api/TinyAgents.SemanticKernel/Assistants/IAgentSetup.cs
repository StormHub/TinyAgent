using Microsoft.SemanticKernel;

namespace TinyAgents.SemanticKernel.Assistants;

internal interface IAgentSetup
{
    string Name { get; }

    string Version { get; }

    string Instructions { get; }

    Kernel Configure(Kernel kernel);
}