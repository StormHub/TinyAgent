namespace TinyAgents.SemanticKernel.Assistants;

public interface IAssistantAgentBuilder
{
    Task<IAssistantAgent> Build(string id, AssistantAgentType agentType, CancellationToken cancellationToken = default);
}