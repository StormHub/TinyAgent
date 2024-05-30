using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TinyAgents.SemanticKernel.OpenAI;

namespace TinyAgents.SemanticKernel.Assistants;

internal sealed class AssistantAgentBuilder : IAssistantAgentBuilder
{
    private readonly AssistantOptions? _options;
    private readonly IServiceProvider _serviceProvider;

    public AssistantAgentBuilder(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var section = configuration.GetSection(nameof(AssistantOptions));
        _options = section.Exists() ? section.Get<AssistantOptions>() : default;

        _serviceProvider = serviceProvider;
    }

    public async Task<IAssistantAgent> Build(CancellationToken cancellationToken = default)
    {
        var builder = _serviceProvider.GetRequiredService<IKernelBuilder>();
        var setup = _serviceProvider.GetRequiredKeyedService<IAgentSetup>(nameof(ChargingLocationsSetup));

        var kernel = setup.Configure(builder).Build();

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
                Name = setup.Name,
                Instructions = setup.Instructions,
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
            Name = setup.Name,
            Instructions = setup.Instructions,
            ExecutionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0,
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            }
        };

        return new AssistantAgent(agent);
    }
}