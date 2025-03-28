using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Agents;
using TinyAgents.SemanticKernel.AzureAI;

namespace TinyAgents.HubHost.Hosting;

internal sealed class AgentHub(
    LocationAgentFactory agentFactory,
    ILogger<AgentHub> logger) : Hub
{
    private const string AgentKey = nameof(AgentKey);
    
    private readonly ILogger _logger = logger;

    public override async Task OnConnectedAsync()
    {
        ChatHistoryAgent? agent = default;
        if (Context.Items.TryGetValue(Context.ConnectionId, out var value)
            && value is ChatHistoryAgent locationAgent)
        {
            agent = locationAgent;
            _logger.LogInformation("Connected {ConnectionId} {Agent}.", Context.ConnectionId, agent.GetType().Name);
        }

        if (agent is null)
        {
            _logger.LogInformation("Connected {ConnectionId} build agents", Context.ConnectionId);
            
            agent = await agentFactory.CreateAgent();
            Context.Items.Add(AgentKey, agent);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(AgentKey, out var item)
            && item is ChatHistoryAgent agent)
        {
            _logger.LogInformation("Disconnected {ConnectionId} {AgentName}", Context.ConnectionId,
                agent.GetType().Name);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ReSharper disable once UnusedMember.Global
    public async Task Restart()
    {
        if (Context.Items.TryGetValue(AgentKey, out var item)
            && item is ChatHistoryAgent agent)
        {
            await agent.DeleteThread(Context.ConnectionAborted);
        }
    }

    // ReSharper disable once UnusedMember.Global
    public async IAsyncEnumerable<ChatMessageContent> Streaming(
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!Context.Items.TryGetValue(AgentKey, out var item)
            || item is not ChatHistoryAgent agent)
            yield break;

        await foreach (var message in agent.Invoke(input, cancellationToken: cancellationToken))
        {
            yield return message;
        }
    }
}