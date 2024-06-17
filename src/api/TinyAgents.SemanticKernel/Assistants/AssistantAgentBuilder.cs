using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using TinyAgents.SemanticKernel.OpenAI;

namespace TinyAgents.SemanticKernel.Assistants;

internal sealed class AssistantAgentBuilder(
    IKernelBuilder kernelBuilder,
    IOptions<OpenAIOptions> options,
    ILogger<AssistantAgentBuilder> logger)
    : IAssistantAgentBuilder
{
    private readonly ILogger _logger = logger;
    private readonly OpenAIOptions _options = options.Value;

    public async Task<IAssistantAgent> Build(
        AssistantAgentType agentType,
        CancellationToken cancellationToken = default)
    {
        var kernel = kernelBuilder.Build();
        var agentSetup = kernel.Services.GetRequiredKeyedService<IAgentSetup>(agentType);
        agentSetup.Configure(kernel);

        var httpClient = kernel.Services.GetRequiredKeyedService<HttpClient>(nameof(OpenAIClient));

        var configuration = new OpenAIAssistantConfiguration(
            _options.ApiKey,
            _options.Uri.ToString())
        {
            HttpClient = httpClient
        };

        OpenAIAssistantAgent? agent = default;
        await foreach (var result in OpenAIAssistantAgent
                           .ListDefinitionsAsync(configuration, cancellationToken: cancellationToken))
            if (string.Equals(agentSetup.Name, result.Name, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(result.Id))
            {
                agent = await OpenAIAssistantAgent.RetrieveAsync(kernel, configuration, result.Id, cancellationToken);
                if (agent.Metadata.TryGetValue(nameof(IAgentSetup.Version), out var version)
                    && string.Equals(version, agentSetup.Version, StringComparison.OrdinalIgnoreCase))
                {
                    agent = await OpenAIAssistantAgent.RetrieveAsync(kernel, configuration, result.Id,
                        cancellationToken);
                    _logger.LogInformation("Retrieve {AgentType} {Id}", result.Name, result.Id);
                    break;
                }

                await agent.DeleteAsync(cancellationToken);
            }

        if (agent is null)
        {
            var definition = new OpenAIAssistantDefinition
            {
                Name = agentSetup.Name,
                Instructions = agentSetup.Instructions,
                ModelId = _options.TextGenerationModelId,
                Metadata = new Dictionary<string, string>
                {
                    {
                        nameof(IAgentSetup.Version), agentSetup.Version
                    }
                }
            };

            agent = await OpenAIAssistantAgent.CreateAsync(
                kernel,
                configuration,
                definition,
                cancellationToken);

            _logger.LogInformation("Creating {AgentType}", agent.GetType().Name);
        }

        return new AssistantAgent(agent);
    }
}