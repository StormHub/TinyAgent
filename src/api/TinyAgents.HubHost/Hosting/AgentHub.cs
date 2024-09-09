using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using TinyAgents.SemanticKernel.Assistants;

namespace TinyAgents.HubHost.Hosting;

internal record MessageContent(string Id, string Role, string Content);

internal sealed class AgentHub(IAssistantAgentBuilder builder, ILogger<AgentHub> logger) : Hub
{
    private const AssistantAgentType AgentType = AssistantAgentType.Locations; // AssistantAgentType.PlanRoutes;
    private readonly ILogger _logger = logger;

    public override async Task OnConnectedAsync()
    {
        IAssistantAgent? agent = default;
        if (Context.Items.TryGetValue(AgentType, out var value))
        {
            agent = value as IAssistantAgent;
            _logger.LogInformation("Connected {ConnectionId} context {AgentType}", Context.ConnectionId, AgentType);
        }

        if (agent is null)
        {
            _logger.LogInformation("Connected {ConnectionId} build {AgentType}", Context.ConnectionId, AgentType);
            agent = await builder.Build(AgentType, Context.ConnectionAborted);
            Context.Items.Add(AgentType, agent);
        }


        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(AgentType, out var item)
            && item is IAssistantAgent agent)
        {
            _logger.LogInformation("Disconnected {ConnectionId} {AgentName}", Context.ConnectionId,
                agent.GetType().Name);
            await agent.DisposeAsync();
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task Restart()
    {
        if (Context.Items.Remove(AgentType, out var item)
            && item is IAssistantAgent agent)
        {
            await agent.DisposeAsync();

            agent = await builder.Build(AgentType, Context.ConnectionAborted);
            Context.Items.Add(AgentType, agent);
        }
    }

    public async IAsyncEnumerable<MessageContent> Streaming(
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!Context.Items.TryGetValue(AgentType, out var item)
            || item is not IAssistantAgent agent)
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