using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Assistants;

namespace TinyAgents.HubHost.Hosting;

internal record MessageContent(string Id, string Role, string Content);

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

    public async IAsyncEnumerable<MessageContent> Streaming(string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var connectionId = Context.ConnectionId;
        if (!Agents.TryGetValue(connectionId, out var agent)) yield break;

        await foreach (var message in agent.Invoke(input, cancellationToken))
        {
            var textContent = message.Items.OfType<TextContent>().FirstOrDefault();
            if (textContent is not null
                && !string.IsNullOrEmpty(textContent.Text))
            {
                string? id = default;
                if (message.Metadata is not null
                    && message.Metadata.TryGetValue(nameof(MessageContent.Id), out var value))
                    id = value?.ToString();

                id ??= Guid.NewGuid().ToString();
                yield return new MessageContent(id, message.Role.Label, textContent.Text);
            }
        }
    }
}