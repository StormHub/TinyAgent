using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace TinyAgents.SemanticKernel.Assistants;

internal sealed class AssistantAgentBuilder : IAssistantAgentBuilder
{
    private readonly IKernelBuilder _kernelBuilder;
    private readonly AssistantOptions? _options;
    private readonly ILogger _logger;

    public AssistantAgentBuilder(
        IKernelBuilder kernelBuilder,
        IConfiguration configuration,
        ILogger<AssistantAgentBuilder> logger)
    {
        var section = configuration.GetSection(nameof(AssistantOptions));
        _options = section.Exists() ? section.Get<AssistantOptions>() : default;

        _kernelBuilder = kernelBuilder;
        _logger = logger;
    }

    public async Task<IAssistantAgent> Build(AssistantAgentType agentType,
        CancellationToken cancellationToken = default)
    {
        var kernel = _kernelBuilder.Build();
        var agentSetup = kernel.Services.GetRequiredKeyedService<IAgentSetup>(agentType);
        agentSetup.Configure(kernel);

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
                Name = agentSetup.Name,
                Instructions = agentSetup.Instructions,
                ModelId = _options.ModelId
            };

            agent = await OpenAIAssistantAgent.CreateAsync(
                kernel,
                configuration,
                definition,
                cancellationToken);
        }

        agent ??= new ChatCompletionAgent
        {
            Kernel = kernel,
            Name = agentSetup.Name,
            Instructions = agentSetup.Instructions,
            ExecutionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0,
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            }
        };

        _logger.LogInformation("Build {AgentType}", agent.GetType().Name);

        return new AssistantAgent(agent);
    }
}