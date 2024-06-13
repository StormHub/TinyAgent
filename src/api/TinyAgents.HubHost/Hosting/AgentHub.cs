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
        var agentId = GetAgentId();
        if (string.IsNullOrEmpty(agentId))
        {
            Context.Abort();
            return;
        }

        if (!Agents.TryGetValue(agentId, out var agent))
        {
            agent = await Build(agentId);
            Agents.AddOrUpdate(agentId, _ => agent, (_, _) => agent);

            _logger.LogInformation("Connected {ConnectionId} {AgentId}", Context.ConnectionId, agentId);
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

    public async Task Restart()
    {
        var agentId = GetAgentId();

        if (!string.IsNullOrEmpty(agentId)
            && Agents.TryRemove(agentId, out var agent))
        {
            await agent.DisposeAsync();

            // Recreate so that no more history
            agent = await Build(agentId);
            Agents.AddOrUpdate(agentId, _ => agent, (_, _) => agent);
        }
    }

    private async Task<IAssistantAgent> Build(string agentId)
    {
        var agent = await builder.Build(agentId, AssistantAgentType.RouteDirections, Context.ConnectionAborted);
        // var agent = await builder.Build(AssistantAgentType.ChargingLocations, Context.ConnectionAborted);
        return agent;
    }

    public async IAsyncEnumerable<MessageContent> Streaming(
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var agentId = GetAgentId();
        if (string.IsNullOrEmpty(agentId)
            || !Agents.TryGetValue(agentId, out var agent))
            yield break;

        await foreach (var message in agent.Invoke(input, cancellationToken))
        {
            var textContent = message.Items.OfType<TextContent>().FirstOrDefault();
            if (string.IsNullOrEmpty(textContent?.Text)) continue;

            string? id = default;
            if (message.Metadata?.TryGetValue(nameof(MessageContent.Id), out var value) ?? false)
                id = value?.ToString();

            id ??= Guid.NewGuid().ToString();
            yield return new MessageContent(id, message.Role.Label, textContent.Text);
        }
    }

    private string? GetAgentId()
    {
        var httpContent = Context.GetHttpContext();
        if (httpContent?.Request.Query.TryGetValue("run", out var value) ?? false) return value.ToString();

        return default;
    }
}