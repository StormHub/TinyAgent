namespace TinyAgents.SemanticKernel.Assistants;

public interface IAssistantAgentBuilder
{
    Task<IAssistantAgent> Build(CancellationToken cancellationToken = default);
}