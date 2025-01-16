using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;

namespace TinyAgents.SemanticKernel.Agents;

public sealed class OrchestratorAgentFactory
{
    private readonly IKernelBuilder _kernelBuilder;
    private readonly OpenAIOptions _openAIOptions;

    public OrchestratorAgentFactory(
        IKernelBuilder kernelBuilder,
        IOptions<OpenAIOptions> options)
    {
        _kernelBuilder = kernelBuilder;
        _openAIOptions = options.Value;
    }

    private const string Name = "OrchestratorAgent";
    
    private const string Instructions =
        """
        You are a helpful AI assistant, collaborating with other assistants.
        If you or any of the other assistants have the final answer or deliverable,
        prefix your response with FINAL ANSWER so the team knows to stop.
        """;
    
    public async Task<AgentGroupChat> CreateAgentGroupChat()
    {
        var kernel = _kernelBuilder.Build();
        var arguments = new KernelArguments(
            new PromptExecutionSettings
            {
                ModelId = _openAIOptions.ModelId,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            });

        var locationAgent = await LocationAgentFactory.CreateChatCompletionAgent(kernel, arguments);
        var searchAgent = await SearchAgentFactory.CreateChatCompletionAgent(kernel, arguments);

        var orchestratorAgent = new ChatCompletionAgent
        {
            Name = Name,
            Instructions = Instructions,
            Arguments = new KernelArguments(
                new PromptExecutionSettings
                {
                    ModelId = _openAIOptions.ModelId,
                    FunctionChoiceBehavior = FunctionChoiceBehavior.None()
                }),
            LoggerFactory = kernel.LoggerFactory
        };

        var groupChat = new AgentGroupChat(orchestratorAgent, locationAgent, searchAgent)
        {
            ExecutionSettings = new AgentGroupChatSettings
            {
                TerminationStrategy = new FinalAnswerTerminationStrategy
                {
                    Agents = [orchestratorAgent]
                }
            }
        };

        return groupChat;
    }
    
    internal sealed class FinalAnswerTerminationStrategy : TerminationStrategy
    {
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        {
            var message = history.LastOrDefault();
            var final = message?.Content?.Contains("FINAL ANSWER") ?? false;
            return Task.FromResult(final);
        }

        public FinalAnswerTerminationStrategy()
        {
            MaximumIterations = 10;
        }
    }
}