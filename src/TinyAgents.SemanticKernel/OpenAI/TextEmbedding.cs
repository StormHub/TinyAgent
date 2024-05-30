using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using TinyAgents.Search;

namespace TinyAgents.SemanticKernel.OpenAI;

internal sealed class TextEmbedding(Kernel kernel, string modelId) : ITextEmbedding
{
    private readonly ITextEmbeddingGenerationService _embeddingGeneration =
        kernel.Services.GetRequiredKeyedService<ITextEmbeddingGenerationService>(modelId);

    public async Task<ReadOnlyMemory<float>> Generate(string text, CancellationToken cancellationToken = default)
    {
        var embedding = await _embeddingGeneration.GenerateEmbeddingAsync(text, kernel, cancellationToken);
        return embedding;
    }
}