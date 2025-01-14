using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TinyAgents.SemanticKernel.Agents;

public sealed class ChatHistoryAgent
{
    private readonly ChatCompletionAgent _agent;
    private readonly ChatHistory _history;
    private readonly ILogger _logger;

    internal ChatHistoryAgent(ChatCompletionAgent agent, ILoggerFactory loggerFactory)
    {
        _agent = agent;
        _history = [];
        _logger = loggerFactory.CreateLogger<ChatHistoryAgent>();
    }

    public void ClearHistory() => _history.Clear();
    
    public async IAsyncEnumerable<ChatMessageContent> Invoke(string input, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _history.Add(new ChatMessageContent(AuthorRole.User, input));
        await foreach (var content in _agent.InvokeAsync(_history, cancellationToken: cancellationToken))
        {
            _logger.LogInformation("{Name} {Content}", _agent.Name, content.Content);
            yield return content;
        }
    }
}