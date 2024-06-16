using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TinyAgents.SemanticKernel.Assistants;

internal sealed class AssistantAgent : IAssistantAgent
{
    private readonly OpenAIAssistantAgent _agent;
    private readonly AgentGroupChat _chat;

    internal AssistantAgent(OpenAIAssistantAgent agent)
    {
        _agent = agent;
        _chat = new AgentGroupChat();
    }

    public async IAsyncEnumerable<ChatMessageContent> Invoke(
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));
        await foreach (var content in _chat.InvokeAsync(_agent, cancellationToken)) yield return content;
    }

    public Task Restart(CancellationToken? cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _chat.IsComplete = true;
        return ValueTask.CompletedTask;
    }
}