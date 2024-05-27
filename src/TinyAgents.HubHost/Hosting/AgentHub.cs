using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using TinyAgents.SemanticKernel.Assistants;

namespace TinyAgents.HubHost.Hosting;

internal sealed class AgentHub(IAssistantAgentBuilder builder) : Hub
{
    private static readonly ConcurrentDictionary<string, IAssistantAgent> Agents = new();
    public override async Task OnConnectedAsync()
    {
        var id = Context.ConnectionId;
        if (!Agents.TryGetValue(id, out var agent))
        {
            agent = await builder.Build();
            Agents.AddOrUpdate(id, _ => agent, (_, _) => agent);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var id = Context.ConnectionId;
        if (Agents.TryRemove(id, out var agent))
        {
            await agent.DisposeAsync();
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async IAsyncEnumerable<string> Streaming(string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var id = Context.ConnectionId;
        if (!Agents.TryGetValue(id, out var agent))
        {
            yield break;
        }
        
        await foreach (var message in agent.Invoke(input, cancellationToken))
        {
            var content = message.Content;
            if (!string.IsNullOrEmpty(content)) yield return content;
        }
    }
}