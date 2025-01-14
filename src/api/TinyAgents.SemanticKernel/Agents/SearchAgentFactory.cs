using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using TinyAgents.Plugins.Search;

namespace TinyAgents.SemanticKernel.Agents;

public sealed class SearchAgentFactory
{
    private readonly IKernelBuilder _kernelBuilder;
    private readonly OpenAIOptions _openAIOptions;

    public SearchAgentFactory(
        IKernelBuilder kernelBuilder,
        IOptions<OpenAIOptions> options)
    {
        _kernelBuilder = kernelBuilder;
        _openAIOptions = options.Value;
    }

    private const string Name = "SearchAgent";
    
    private const string Instructions =
        """
        You are an assistant helping users to search for electric vehicle charger types.
        """;
    
    public Task<ChatHistoryAgent> CreateAgent(CancellationToken cancellationToken = default)
    {
        var kernel = _kernelBuilder.Build();
        kernel.Plugins.AddFromObject(kernel.Services.GetRequiredService<SearchPlugin>());

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

        var agent = new ChatHistoryAgent(chatCompletionAgent, kernel.LoggerFactory);
        return Task.FromResult(agent);
    }
}