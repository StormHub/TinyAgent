using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Agents;

namespace TinyAgents.HubHost.Hosting;

internal  sealed class AssistantHub(LocationAgentFactory factory, ILogger<AgentHub> logger) : Hub
{
    private const string AssistantKey = nameof(AssistantKey);
    
    private readonly ILogger _logger = logger;

    public override async Task OnConnectedAsync()
    {
        AssistantAgent? agent = default;
        if (Context.Items.TryGetValue(Context.ConnectionId, out var value)
            && value is AssistantAgent locationAgent)
        {
            agent = locationAgent;
            _logger.LogInformation("Connected {ConnectionId} {Assistant}.", Context.ConnectionId, agent.GetType().Name);
        }

        if (agent is null)
        {
            _logger.LogInformation("Connected {ConnectionId} build agents", Context.ConnectionId);
            agent = await factory.CreateLocationAssistant(Context.ConnectionAborted);
            await agent.NewThread(Context.ConnectionAborted);
            Context.Items.Add(AssistantKey, agent);
        }

        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(AssistantKey, out var item)
            && item is AssistantAgent agent)
        {
            _logger.LogInformation("Disconnected {ConnectionId} {AgentName}", Context.ConnectionId,
                agent.GetType().Name);
            await agent.DisposeAsync();
        }

        await base.OnDisconnectedAsync(exception);
    }
    
    // ReSharper disable once UnusedMember.Global
    public async Task Restart()
    {
        if (Context.Items.TryGetValue(AssistantKey, out var item)
            && item is AssistantAgent agent)
        {
            await agent.NewThread();
        }
    }
    
    // ReSharper disable once UnusedMember.Global
    public async IAsyncEnumerable<ChatMessageContent> Streaming(
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!Context.Items.TryGetValue(AssistantKey, out var item)
            || item is not AssistantAgent agent)
            yield break;

        await foreach (var message in agent.Invoke(input, cancellationToken))
        {
            yield return message;
        }
    }
}