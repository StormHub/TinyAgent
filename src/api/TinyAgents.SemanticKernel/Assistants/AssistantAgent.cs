using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TinyAgents.SemanticKernel.Assistants;

internal sealed class AssistantAgent : IAssistantAgent
{
    private readonly AgentGroupChat _chat;

    internal AssistantAgent(Agent[] agents, ILoggerFactory loggerFactory)
    {
        _chat = new AgentGroupChat(agents)
        {
            LoggerFactory = loggerFactory
        };
    }

    public async IAsyncEnumerable<ChatMessageContent> Invoke(
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));
        await foreach (var content in _chat.InvokeAsync(cancellationToken)) yield return content;
    }

    public ValueTask DisposeAsync()
    {
        _chat.IsComplete = true;
        return ValueTask.CompletedTask;
    }
}