using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TinyAgents.SemanticKernel.Agents;

public sealed class ChatHistoryAgent
{
    private readonly ChatCompletionAgent _agent;
    private readonly ChatHistoryAgentThread _agentThread;
    private readonly ILogger _logger;

    internal ChatHistoryAgent(ChatCompletionAgent agent, ChatHistoryAgentThread? agentThread, ILogger logger)
    {
        _agent = agent;
        _agentThread = agentThread ?? new();
        _logger = logger;
    }

    public async Task DeleteThread(CancellationToken cancellationToken = default)
    {
        await _agentThread.DeleteAsync(cancellationToken);
    }

    public async IAsyncEnumerable<ChatMessageContent> Invoke(
        string input,
        KernelArguments? arguments = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var message = new ChatMessageContent(AuthorRole.User, input);
        var options = arguments != null
            ? new AgentInvokeOptions { KernelArguments = arguments }
            : default;
        await foreach (var response in _agent.InvokeAsync(
                           [message],
                           _agentThread,
                           options,
                           cancellationToken))
        {
            _logger.LogInformation("{Name} {Content}", _agent.Name, response.Message.Content);
            yield return response.Message;
        }
    }
}