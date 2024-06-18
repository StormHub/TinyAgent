using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;

namespace TinyAgents.SemanticKernel.OpenAI.Plugins;

internal sealed class PlannerPlugin
{
    [KernelFunction(nameof(PlanRoutes))]
    [Description("Plan electric vehicle routes base on charging stations of the electric vehicle charger types from rom origin to destination address in Australia")]
    public async Task<FunctionCallingStepwisePlannerResult?> PlanRoutes(
        [Description("User question input")] string question,
        Kernel kernel, CancellationToken cancellationToken = default)
    {
        var options = new FunctionCallingStepwisePlannerOptions
        {
            MaxIterations = 15,
        };
        var planner = new FunctionCallingStepwisePlanner(options);
        var result = await planner.ExecuteAsync(kernel, question, cancellationToken: cancellationToken);
        return result;
    }
}