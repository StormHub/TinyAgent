using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TinyAgents.Locations;

namespace TinyAgents.SemanticKernel.Assistants;

internal sealed class AssistantAgentBuilder(IKernelBuilder kernelBuilder, IOptions<AssistantOptions>? options)
    : IAssistantAgentBuilder
{
    private const string Name = "Assistant";

    private const string Instructions =
        """
        You are an assistent helping users looking for GPS positions from Postal address, postcode, suburbs in Australia.
        The goal is to find the closest GPS postion for users.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        """;

    private readonly AssistantOptions? _options = options?.Value;

    public async Task<IAssistantAgent> Build(CancellationToken cancellationToken = default)
    {
        var kernel = kernelBuilder.Build();

        kernel.AddLocationPlugin();

        KernelAgent? agent = default;
        if (_options is not null)
        {
            var httpClient = kernel.Services.GetRequiredKeyedService<HttpClient>(nameof(OpenAIClient));

            var configuration = new OpenAIAssistantConfiguration(
                _options.ApiKey,
                _options.Uri.ToString())
            {
                HttpClient = httpClient
            };

            var definition = new OpenAIAssistantDefinition
            {
                Instructions = Instructions,
                Name = Name,
                ModelId = _options.ModelId
            };

            agent = await OpenAIAssistantAgent.CreateAsync(
                kernel,
                configuration,
                definition,
                cancellationToken);
        }

        if (agent is null)
        {
            var settings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0,
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            agent = new ChatCompletionAgent
            {
                Kernel = kernel,
                Name = Name,
                Instructions = Instructions,
                ExecutionSettings = settings
            };
        }

        return new AssistantAgent(agent);
    }
}