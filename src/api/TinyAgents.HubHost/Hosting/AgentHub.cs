using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Agents;

namespace TinyAgents.HubHost.Hosting;

internal record MessageContent(string Id, string Role, string Content);

internal sealed class AgentHub(LocationAgentFactory factory, ILogger<AgentHub> logger) : Hub
{
    private const string AgentType = nameof(AgentType);
    
    private readonly ILogger _logger = logger;

    public override async Task OnConnectedAsync()
    {
        LocationAgent? agent = default;
        if (Context.Items.TryGetValue(Context.ConnectionId, out var value)
            && value is LocationAgent locationAgent)
        {
            agent = locationAgent;
            _logger.LogInformation("Connected {ConnectionId} {Assistant}.", Context.ConnectionId, agent.GetType().Name);
        }

        if (agent is null)
        {
            _logger.LogInformation("Connected {ConnectionId} build agents", Context.ConnectionId);
            agent = await factory.CreateAgent(Context.ConnectionAborted);
            Context.Items.Add(AgentType, agent);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(AgentType, out var item)
            && item is AssistantAgent agent)
        {
            _logger.LogInformation("Disconnected {ConnectionId} {AgentName}", Context.ConnectionId,
                agent.GetType().Name);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ReSharper disable once UnusedMember.Global
    public Task Restart()
    {
        if (Context.Items.TryGetValue(AgentType, out var item)
            && item is LocationAgent agent)
        {
            agent.ClearHistory();
        }
        return Task.CompletedTask;
    }

    // ReSharper disable once UnusedMember.Global
    public async IAsyncEnumerable<MessageContent> Streaming(
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!Context.Items.TryGetValue(AgentType, out var item)
            || item is not LocationAgent agent)
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
}