using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using TinyAgents.HubHost.Agents;

namespace TinyAgents.HubHost.Hosting;

internal sealed class AgentHub : Hub
{
    private readonly TripAssistant _agentBuilder;
    private static readonly ConcurrentDictionary<string, TripAssistant.Session> _sessions = new();

    public AgentHub(TripAssistant agentBuilder)
    {
        _agentBuilder = agentBuilder;
    }

    public override async Task OnConnectedAsync()
    {
        var id = Context.ConnectionId;
        if (!_sessions.TryGetValue(id, out var session))
        {
            session = await _agentBuilder.CreateSession();
            _sessions.AddOrUpdate(id, (k) => session, (k, v) => session);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var id = Context.ConnectionId;
        if (_sessions.TryRemove(id, out var session))
        {
            await session.DisposeAsync();
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async IAsyncEnumerable<string> Streaming(string input, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var id = Context.ConnectionId;
        if (_sessions.TryGetValue(id, out var session))
        {
            await foreach (var message in session.Invoke(input, cancellationToken))
            {
                var content = message.Content;
                if (!string.IsNullOrEmpty(content))
                {
                    yield return content;
                }
            }
        }
    }
}
