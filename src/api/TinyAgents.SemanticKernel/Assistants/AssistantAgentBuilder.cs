using System.ClientModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using OpenAI;
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

        var credential = new ApiKeyCredential(_options.ApiKey);
        var provider = OpenAIClientProvider.ForAzureOpenAI(credential, _options.Uri, httpClient);

        OpenAIAssistantAgent? agent = default;
        await foreach (var result in OpenAIAssistantAgent
                           .ListDefinitionsAsync(provider, cancellationToken))
            if (string.Equals(agentSetup.Name, result.Name, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(result.Id))
            {
                agent = await OpenAIAssistantAgent.RetrieveAsync(kernel, provider, result.Id, cancellationToken);

                if (agent.Definition.Metadata is not null
                    && agent.Definition.Metadata.TryGetValue(nameof(IAgentSetup.Version), out var version)
                    && string.Equals(version, agentSetup.Version, StringComparison.OrdinalIgnoreCase))
                {
                    agent = await OpenAIAssistantAgent.RetrieveAsync(kernel, provider, result.Id,
                        cancellationToken);
                    _logger.LogInformation("Retrieve {AgentType} {Id}", result.Name, result.Id);
                    break;
                }

                await agent.DeleteAsync(cancellationToken);
            }

        if (agent is null)
        {
            var definition = new OpenAIAssistantDefinition(_options.TextGenerationModelId)
            {
                Name = agentSetup.Name,
                Instructions = agentSetup.Instructions,
                Metadata = new Dictionary<string, string>
                {
                    {
                        nameof(IAgentSetup.Version), agentSetup.Version
                    }
                }
                // Assistant 2024-02-15-preview does not support file tool
                // keep it disabled until 2024-05-01-preview is in use
                // EnableRetrieval = true,  
            };

            agent = await OpenAIAssistantAgent.CreateAsync(
                kernel,
                provider,
                definition,
                cancellationToken);

            _logger.LogInformation("Creating {AgentType}", agent.GetType().Name);
        }

        var loggerFactory = kernel.Services.GetRequiredService<ILoggerFactory>();
        
        agent.PollingOptions.RunPollingInterval = TimeSpan.FromSeconds(1);
        agent.PollingOptions.RunPollingBackoffThreshold = 1;
        
        return new AssistantAgent(agent, loggerFactory);
    }
}