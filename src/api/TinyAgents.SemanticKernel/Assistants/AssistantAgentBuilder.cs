using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;

namespace TinyAgents.SemanticKernel.Assistants;

internal sealed class AssistantAgentBuilder(
    IKernelBuilder kernelBuilder,
    ILogger<AssistantAgentBuilder> logger)
    : IAssistantAgentBuilder
{
    private readonly ILogger _logger = logger;

    public async Task<IAssistantAgent> Build(CancellationToken cancellationToken = default)
    {
        var kernel = kernelBuilder.Build();
        Agent[] agents = await BuildAgents(kernel, cancellationToken)
            .ToArrayAsync(cancellationToken);
        
        var loggerFactory = kernel.Services.GetRequiredService<ILoggerFactory>();
        return new AssistantAgent(agents, loggerFactory);
    }

    private async IAsyncEnumerable<OpenAIAssistantAgent> BuildAgents(Kernel kernel, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var provider = kernel.Services.GetRequiredService<OpenAIClientProvider>();

        foreach (var agentSetup in kernel.Services.GetServices<IAgentSetup>())
        {
            agentSetup.Configure(kernel);
            
            await foreach (var result in OpenAIAssistantAgent
                               .ListDefinitionsAsync(provider, cancellationToken))
            {
                if (!string.Equals(agentSetup.Name, result.Name, StringComparison.OrdinalIgnoreCase)) continue;
                
                var assistantAgent =
                    await OpenAIAssistantAgent.RetrieveAsync(provider, result.Id, kernel, default, default, cancellationToken);
                var deleted = await assistantAgent.DeleteAsync(cancellationToken);
                if (deleted)
                    _logger.LogInformation("Removed OpenAI assistant {Id}", assistantAgent.Id);
                else
                    _logger.LogWarning("Unable to remove OpenAI assistant {Id}", assistantAgent.Id);
            }

            var definition = new OpenAIAssistantDefinition("gpt-4o")
            {
                Name = agentSetup.Name,
                Instructions = agentSetup.Instructions,
                Temperature = 0,
                TopP = 0,
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
                provider,
                definition,
                kernel,
                default,
                cancellationToken);

            _logger.LogInformation("{AgentType} created {Id}", agent.GetType().Name, agent.Id);

            yield return agent;
        }
    }
}