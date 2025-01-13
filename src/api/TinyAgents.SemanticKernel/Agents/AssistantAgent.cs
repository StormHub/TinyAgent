using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TinyAgents.SemanticKernel.Agents;

public sealed class AssistantAgent : IAsyncDisposable
{
    private readonly OpenAIAssistantAgent _agent;
    private readonly ILogger _logger;
    private string? _threadId;

    internal AssistantAgent(OpenAIAssistantAgent agent, ILoggerFactory loggerFactory)
    {
        _agent = agent;
        _logger = loggerFactory.CreateLogger<AssistantAgent>();
        _threadId = default;
    }

    public async Task NewThread(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_threadId))
        {
            await _agent.DeleteThreadAsync(_threadId, cancellationToken);
        }
        _threadId = await _agent.CreateThreadAsync(cancellationToken);
    }

    public async IAsyncEnumerable<ChatMessageContent> Invoke(
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_threadId))
        {
            yield break;
        }
        
        await _agent.AddChatMessageAsync(_threadId, new ChatMessageContent(AuthorRole.User, input), cancellationToken);
        await foreach (var content in _agent.InvokeAsync(_threadId, cancellationToken: cancellationToken))
        {
            _logger.LogInformation("{Thread} {Content}", _threadId, content.Content);
            yield return content;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!string.IsNullOrEmpty(_threadId))
        {
            await _agent.DeleteThreadAsync(_threadId);
            _threadId = default;
        }
    }
}