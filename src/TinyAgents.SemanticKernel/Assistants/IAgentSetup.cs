using Microsoft.SemanticKernel;

namespace TinyAgents.SemanticKernel.Assistants;

public interface IAgentSetup
{
    string Name { get; }

    string Instructions { get; }

    IKernelBuilder Configure(IKernelBuilder builder);
}