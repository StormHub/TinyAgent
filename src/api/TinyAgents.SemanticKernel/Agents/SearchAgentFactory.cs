using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
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
        You are an assistant helping users search the web for the latest information.
        """;
    
    public async Task<ChatHistoryAgent> CreateAgent(ChatHistory? history = default)
    {
        var kernel = _kernelBuilder.Build();
        var arguments = new KernelArguments(
            new PromptExecutionSettings
            {
                ModelId = _openAIOptions.ModelId,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            });
        var chatCompletionAgent = await CreateChatCompletionAgent(kernel, arguments);
        return new ChatHistoryAgent(chatCompletionAgent, history);
    }
    
    internal static Task<ChatCompletionAgent> CreateChatCompletionAgent(Kernel kernel, KernelArguments arguments)
    {
        kernel.Plugins.AddFromObject(kernel.Services.GetRequiredService<SearchPlugin>());

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
}