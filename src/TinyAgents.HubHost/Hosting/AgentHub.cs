using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using TinyAgents.HubHost.Agents;

namespace TinyAgents.HubHost.Hosting;

internal sealed class AgentHub(TripAssistant agentBuilder) : Hub
{
    private static readonly ConcurrentDictionary<string, TripAssistant.Session> Sessions = new();

    public override async Task OnConnectedAsync()
    {
        var id = Context.ConnectionId;
        if (!Sessions.TryGetValue(id, out var session))
        {
            session = await agentBuilder.CreateSession();
            Sessions.AddOrUpdate(id, _ => session, (_, _) => session);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var id = Context.ConnectionId;
        if (Sessions.TryRemove(id, out var session)) await session.DisposeAsync();

        await base.OnDisconnectedAsync(exception);
    }

    public async IAsyncEnumerable<string> Streaming(string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var id = Context.ConnectionId;
        if (Sessions.TryGetValue(id, out var session))
            await foreach (var message in session.Invoke(input, cancellationToken))
            {
                var content = message.Content;
                if (!string.IsNullOrEmpty(content)) yield return content;
            }
    }
}