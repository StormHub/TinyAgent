using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using TinyAgents.Plugins.Maps;
using TinyAgents.SemanticKernel.Agents;

namespace TinyAgents.SemanticKernel.AzureAI;

public sealed class LocationAgentFactory(
    IKernelBuilder kernelBuilder,
    IOptions<AzureConfiguration> options,
    ILoggerFactory loggerFactory)
{
    private readonly AzureConfiguration _azureConfiguration = options.Value;

    private const string Name = "LocationAgent";
    
    private const string Instructions =
        """
        You are an assistant helping users to find driving routes from a given origin postal address to a destinationf postal address.
        """;

    public async Task<ChatHistoryAgent> CreateAgent(KernelArguments? arguments = default, ChatHistoryAgentThread? agentThread = default)
    {
        var kernel = kernelBuilder.Build();
        arguments ??= new KernelArguments(
            new AzureOpenAIPromptExecutionSettings
            {
                ModelId = _azureConfiguration.ModelId,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = 0
            });
        var chatCompletionAgent = await CreateChatCompletionAgent(kernel, arguments);
        return new ChatHistoryAgent(chatCompletionAgent, agentThread, loggerFactory.CreateLogger<ChatHistoryAgent>());
    }

    internal static Task<ChatCompletionAgent> CreateChatCompletionAgent(Kernel kernel, KernelArguments arguments)
    {
        kernel.Plugins.AddFromObject(kernel.Services.GetRequiredService<LocationPlugin>());
        kernel.Plugins.AddFromObject(kernel.Services.GetRequiredService<RoutingPlugin>());

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