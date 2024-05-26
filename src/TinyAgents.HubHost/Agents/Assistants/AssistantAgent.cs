using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TinyAgents.HubHost.Agents.Assistants;

internal sealed class AssistantAgent : IAsyncDisposable
{
    private readonly KernelAgent _agent;
    private readonly AgentGroupChat _chat;

    internal AssistantAgent(KernelAgent agent)
    {
        _agent = agent;
        _chat = new AgentGroupChat(_agent);
    }

    public async IAsyncEnumerable<ChatMessageContent> Invoke(string input, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));
        await foreach (var content in _chat.InvokeAsync(cancellationToken))
        {
            yield return content;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_agent is OpenAIAssistantAgent openAIAssistantAgent)
        {
            await openAIAssistantAgent.DeleteAsync();
        }
        _chat.IsComplete = true;
    }
}