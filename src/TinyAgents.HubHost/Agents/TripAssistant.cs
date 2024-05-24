using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TinyAgents.HubHost.Agents;

internal sealed class TripAssistant(IKernelBuilder kernelBuilder, string modelId)
{
    private const string Name = "TripAssistant";

    private const string Instructions =
        """
        You are an assistent helping users looking for GPS positions from Postal address, postcode, suburbs in Australia.
        The goal is to find the closest GPS postion for users.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        """;

    internal async Task<Session> CreateSession(CancellationToken cancellationToken = default)
    {
        var kernel = kernelBuilder.Build();

        await LocationPlugin.AddTo(kernel);

        var configuration =
            kernel.Services.GetRequiredKeyedService<OpenAIAssistantConfiguration>(nameof(OpenAIAssistantAgent));
        var agent = await OpenAIAssistantAgent.CreateAsync(
            kernel,
            configuration,
            new OpenAIAssistantDefinition
            {
                Instructions = Instructions,
                Name = Name,
                ModelId = modelId
            },
            cancellationToken);

        return new Session(agent);
    }

    internal sealed class Session : IAsyncDisposable
    {
        private readonly OpenAIAssistantAgent _agent;
        private readonly AgentGroupChat _chat;

        public Session(OpenAIAssistantAgent agent)
        {
            _agent = agent;
            _chat = new AgentGroupChat(_agent);
        }

        public async ValueTask DisposeAsync()
        {
            await _agent.DeleteAsync();
            _chat.IsComplete = true;
        }

        public async IAsyncEnumerable<ChatMessageContent> Invoke(string input,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));
            await foreach (var content in _chat.InvokeAsync(cancellationToken)) yield return content;
        }
    }
}