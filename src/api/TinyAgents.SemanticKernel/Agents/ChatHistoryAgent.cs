using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TinyAgents.SemanticKernel.Agents;

public sealed class ChatHistoryAgent
{
    private readonly ChatCompletionAgent _agent;
    private readonly ILogger _logger;

    internal ChatHistoryAgent(ChatCompletionAgent agent, ChatHistory? history = default)
    {
        _agent = agent;
        History = history ?? [];
        _logger = agent.LoggerFactory.CreateLogger<ChatHistoryAgent>();
    }

    public ChatHistory History { get; }

    public async IAsyncEnumerable<ChatMessageContent> Invoke(
        KernelArguments? arguments = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var content in _agent.InvokeAsync(History, arguments, cancellationToken: cancellationToken))
        {
            History.Add(content);
            _logger.LogInformation("{Name} {Content}", _agent.Name, content.Content);
            yield return content;
        }
    }
}