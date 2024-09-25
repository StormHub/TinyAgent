using System.ClientModel.Primitives;
using System.Diagnostics;

namespace TinyAgents.SemanticKernel.OpenAI;

internal sealed class OpenAIPipelinePolicy : PipelinePolicy
{
    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        ProcessNext(message, pipeline, currentIndex);
        var response = message.Response;
        if (response is not null) Debug.WriteLine("here");
    }

    public override async ValueTask ProcessAsync(
        PipelineMessage message,
        IReadOnlyList<PipelinePolicy> pipeline,
        int currentIndex)
    {
        await ProcessNextAsync(message, pipeline, currentIndex);
        var response = message.Response;
        if (response is not null) Debug.WriteLine("here");
    }
}