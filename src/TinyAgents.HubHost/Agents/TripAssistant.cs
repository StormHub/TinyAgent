using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TinyAgents.HubHost.Agents;

internal sealed class TripAssistant
{
    private readonly IKernelBuilder _kernelBuilder;
    private readonly string _modelId;

    public TripAssistant(IKernelBuilder kernelBuilder, string modelId)
    {
        _kernelBuilder = kernelBuilder;
        _modelId = modelId;
    }

    private const string Name = "HealthcareAssistant";
    private const string Instructions =
        """
        You are an assistent helping users to search for healthcare providers including Dentists, Chiropractors, Physiotherapists, Optical and Hospitals in Australia.
        The goal is to find quality healthcare providers that cater to the user’s specific health needs and are conveniently located nearby.
        Provide at least three but no more than five proposals per response if possible.
        Acknowledge location address for postcodes.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        """;

    internal sealed class Session : IAsyncDisposable
    {
        private readonly OpenAIAssistantAgent _agent;
        private readonly AgentGroupChat _chat;

        public Session(OpenAIAssistantAgent agent)
        {
            _agent = agent;
            _chat = new AgentGroupChat(_agent);
        }

        public async IAsyncEnumerable<ChatMessageContent> Invoke(string input, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));
            await foreach (var content in _chat.InvokeAsync(cancellationToken))
            {
                yield return content;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _agent.DeleteAsync();
            _chat.IsComplete = true;
        }
    }

    internal async Task<Session> CreateSession(CancellationToken cancellationToken = default)
    {
        var kernel = _kernelBuilder.Build();

        var locationPlugin = await LocationPlugin.Create(kernel);
        kernel.Plugins.AddFromObject(locationPlugin);

        // var plugin = await SearchPlugin.Create(kernel);
        // kernel.Plugins.AddFromObject(plugin);

        var configuration = kernel.Services.GetRequiredKeyedService<OpenAIAssistantConfiguration>(nameof(OpenAIAssistantAgent));
        var agent = await OpenAIAssistantAgent.CreateAsync(
            kernel,
            configuration,
            new()
            {
                Instructions = Instructions,
                Name = Name,
                ModelId = _modelId,
            },
            cancellationToken: cancellationToken);

        return new Session(agent);
    }
}
