using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TinyAgents.SemanticKernel.Agents;

public sealed class AgentProxy
{
    private readonly ChatCompletionAgent _agent;
    private readonly ChatHistory _history;
    private readonly ILogger _logger;

    internal AgentProxy(ChatCompletionAgent agent, ChatHistory? history = default)
    {
        _agent = agent;
        _history = history ?? [];
        _logger = agent.LoggerFactory.CreateLogger<AgentProxy>();
    }

    public void ClearHistory() => _history.Clear();

    public async IAsyncEnumerable<ChatMessageContent> Invoke(
        string input, 
        KernelArguments? arguments = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messageContent = new ChatMessageContent(AuthorRole.User, input);
        await foreach (var content in Invoke(messageContent, arguments, cancellationToken))
        {
            yield return content;
        }
    }
    
    public async IAsyncEnumerable<ChatMessageContent> Invoke(
        ChatMessageContent messageContent, 
        KernelArguments? arguments = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _history.Add(messageContent);
        await foreach (var content in Invoke(arguments, cancellationToken: cancellationToken))
        {
            yield return content;
        }
    }
    
    public async IAsyncEnumerable<ChatMessageContent> Invoke(
        KernelArguments? arguments = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var content in _agent.InvokeAsync(_history, arguments, cancellationToken: cancellationToken))
        {
            _history.Add(content);
            _logger.LogInformation("{Name} {Content}", _agent.Name, content.Content);
            yield return content;
        }
    }
}