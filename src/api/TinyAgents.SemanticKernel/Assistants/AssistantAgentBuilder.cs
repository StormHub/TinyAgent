using Azure.Identity;
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

        var httpClient = kernel.Services.GetRequiredKeyedService<HttpClient>(_options.Uri);

        var provider = ! string.IsNullOrEmpty(_options.ApiKey) 
            ? OpenAIClientProvider.ForAzureOpenAI(_options.ApiKey, _options.Uri, httpClient) 
            : OpenAIClientProvider.ForAzureOpenAI(new DefaultAzureCredential(), _options.Uri, httpClient);

        await foreach (var result in OpenAIAssistantAgent
                           .ListDefinitionsAsync(provider, cancellationToken))
            if (string.Equals(agentSetup.Name, result.Name, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(result.Id))
            {
                var assistantAgent = await OpenAIAssistantAgent.RetrieveAsync(kernel, provider, result.Id, cancellationToken);
                await assistantAgent.DeleteAsync(cancellationToken);
            }

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

        var agent = await OpenAIAssistantAgent.CreateAsync(
            kernel,
            provider,
            definition,
            cancellationToken);

        _logger.LogInformation("Creating {AgentType}", agent.GetType().Name);

        var loggerFactory = kernel.Services.GetRequiredService<ILoggerFactory>();

        agent.PollingOptions.RunPollingInterval = _options.RunPollingInterval;
        agent.PollingOptions.RunPollingBackoff = _options.RunPollingBackoff;
        agent.PollingOptions.RunPollingBackoffThreshold = _options.DefaultPollingBackoffThreshold;
        
        return new AssistantAgent(agent, loggerFactory);
    }
}