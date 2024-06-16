using Microsoft.SemanticKernel;

namespace TinyAgents.SemanticKernel.Assistants;

public interface IAssistantAgent : IAsyncDisposable
{
    IAsyncEnumerable<ChatMessageContent> Invoke(string input, CancellationToken cancellationToken);
}