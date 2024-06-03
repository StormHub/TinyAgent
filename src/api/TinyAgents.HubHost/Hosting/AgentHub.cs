using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Assistants;

namespace TinyAgents.HubHost.Hosting;

internal sealed class AgentHub(IAssistantAgentBuilder builder, ILogger<AgentHub> logger) : Hub
{
    private static readonly ConcurrentDictionary<string, IAssistantAgent> Agents = new();
    private readonly ILogger _logger = logger;

    public override async Task OnConnectedAsync()
    {
        var id = Context.ConnectionId;
        if (!Agents.TryGetValue(id, out var agent))
        {
            // agent = await builder.Build(AssistantAgentType.RouteDirections, Context.ConnectionAborted);
            agent = await builder.Build(AssistantAgentType.ChargingLocations, Context.ConnectionAborted);
            Agents.AddOrUpdate(id, _ => agent, (_, _) => agent);
            
            _logger.LogInformation("Connected {Id}", id);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var id = Context.ConnectionId;
        if (Agents.TryRemove(id, out var agent)) await agent.DisposeAsync();
        _logger.LogInformation("Disconnected {Id}", id);

        await base.OnDisconnectedAsync(exception);
    }

    public async IAsyncEnumerable<ChatMessageContent> Streaming(string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var id = Context.ConnectionId;
        if (!Agents.TryGetValue(id, out var agent)) yield break;

        await foreach (var message in agent.Invoke(input, cancellationToken))
        {
            yield return message;
        }
    }
}