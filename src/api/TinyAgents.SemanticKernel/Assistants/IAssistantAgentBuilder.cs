namespace TinyAgents.SemanticKernel.Assistants;

public interface IAssistantAgentBuilder
{
    Task<IAssistantAgent> Build(AssistantAgentType agentType, CancellationToken cancellationToken = default);
}