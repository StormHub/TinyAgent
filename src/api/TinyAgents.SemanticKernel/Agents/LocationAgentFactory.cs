using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using TinyAgents.Plugins.Maps;

namespace TinyAgents.SemanticKernel.Agents;

public sealed class LocationAgentFactory(
    IKernelBuilder kernelBuilder,
    IOptions<OpenAIOptions> options,
    ILogger<LocationAgentFactory> logger)
{
    private readonly OpenAIOptions _openAIOptions = options.Value;
    private readonly ILogger _logger = logger;

    private const string Name = "LocationAgent";
    
    private const string Instructions =
        """
        You are an assistant helping users to find GPS locations from postal address.
        """;

    public async Task<AgentProxy> CreateAgent(ChatHistory? history = default, KernelArguments? arguments = default)
    {
        var kernel = kernelBuilder.Build();
        arguments ??= new KernelArguments(
            new AzureOpenAIPromptExecutionSettings
            {
                ModelId = _openAIOptions.Agents.ModelId,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = 0
            });
        var chatCompletionAgent = await CreateChatCompletionAgent(kernel, arguments);
        return new AgentProxy(chatCompletionAgent, history);
    }

    internal static Task<ChatCompletionAgent> CreateChatCompletionAgent(Kernel kernel, KernelArguments arguments)
    {
        kernel.Plugins.AddFromObject(kernel.Services.GetRequiredService<LocationPlugin>());

        var chatCompletionAgent = new ChatCompletionAgent
        {
            Name = Name,
            Instructions = Instructions,
            Kernel = kernel,
            Arguments = arguments,
            LoggerFactory = kernel.LoggerFactory
        };

        return Task.FromResult(chatCompletionAgent);
    }

    public async Task<AssistantAgent> CreateAssistant(CancellationToken cancellationToken = default)
    {
        var kernel = kernelBuilder.Build();
        kernel.Plugins.AddFromObject(kernel.Services.GetRequiredService<LocationPlugin>());
        
        var provider = kernel.Services.GetRequiredService<OpenAIClientProvider>();

        await foreach (var definition in OpenAIAssistantAgent.ListDefinitionsAsync(provider, cancellationToken))
        {
            if (string.Equals(definition.Name, Name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(definition.ModelId, _openAIOptions.Agents.ModelId))
            {
                _logger.LogInformation("OpenAIAssistantAgent {Name} {Id} exists", definition.Name, definition.Id);
                var openAIAssistantAgent = await OpenAIAssistantAgent.RetrieveAsync(
                    provider,
                    definition.Id,
                    kernel,
                    default,
                    default,
                    cancellationToken);
                return new AssistantAgent(openAIAssistantAgent);
            }
        }

        var assistantDefinition = new OpenAIAssistantDefinition(_openAIOptions.Agents.ModelId)
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

        return new AssistantAgent(agent);
    }
}