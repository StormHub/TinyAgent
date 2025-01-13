using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using TinyAgents.Plugins.Maps;

namespace TinyAgents.SemanticKernel.Agents;

public sealed class LocationAgentFactory
{
    private readonly IKernelBuilder _kernelBuilder;
    private readonly OpenAIOptions _openAIOptions;
    private readonly ILogger _logger;

    public LocationAgentFactory(
        IKernelBuilder kernelBuilder,
        IOptions<OpenAIOptions> options,
        ILogger<LocationAgentFactory> logger)
    {
        _kernelBuilder = kernelBuilder;
        _openAIOptions = options.Value;
        _logger = logger;
    }

    private const string Name = "LocationAssistant";
    
    private const string Instructions =
        """
        You are an assistant helping users to find GPS locations from postal address in Australia.
        You're laser focused on the goal at hand.
        Answer questions only from given facts.
        """;

    public Task<LocationAgent> CreateAgent(CancellationToken cancellationToken = default)
    {
        var kernel = _kernelBuilder.Build();
        kernel.Plugins.AddFromObject(kernel.Services.GetRequiredService<MapPlugin>());

        var chatCompletionAgent = new ChatCompletionAgent
        {
            Name = Name,
            Instructions = Instructions,
            Kernel = kernel,
            Arguments = new KernelArguments(
                new PromptExecutionSettings
                {
                    ModelId = _openAIOptions.ModelId,
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                }),
            LoggerFactory = kernel.LoggerFactory
        };

        var agent = new LocationAgent(chatCompletionAgent, kernel.LoggerFactory);
        return Task.FromResult(agent);
    }

    public async Task<AssistantAgent> CreateAssistant(CancellationToken cancellationToken = default)
    {
        var kernel = _kernelBuilder.Build();
        kernel.Plugins.AddFromObject(kernel.Services.GetRequiredService<MapPlugin>());
        
        var provider = kernel.Services.GetRequiredService<OpenAIClientProvider>();

        await foreach (var definition in OpenAIAssistantAgent.ListDefinitionsAsync(provider, cancellationToken))
        {
            if (string.Equals(definition.Name, Name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(definition.ModelId, _openAIOptions.ModelId))
            {
                _logger.LogInformation("OpenAIAssistantAgent {Name} {Id} exists", definition.Name, definition.Id);
                var openAIAssistantAgent = await OpenAIAssistantAgent.RetrieveAsync(
                    provider,
                    definition.Id,
                    kernel,
                    default,
                    default,
                    cancellationToken);
                return new AssistantAgent(openAIAssistantAgent, kernel.LoggerFactory);
            }
        }

        var assistantDefinition = new OpenAIAssistantDefinition(_openAIOptions.ModelId)
        {
            Name = Name,
            Instructions = Instructions,
            Temperature = 0,
            TopP = 0,
        };

        var agent = await OpenAIAssistantAgent.CreateAsync(
            provider,
            assistantDefinition,
            kernel,
            default,
            cancellationToken);
        
        _logger.LogInformation("OpenAIAssistantAgent {Name} {Id} created", agent.Name, agent.Id);

        return new AssistantAgent(agent, kernel.LoggerFactory);
    }
}