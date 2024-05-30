namespace TinyAgents.Search;

public interface ITextEmbedding
{
    Task<ReadOnlyMemory<float>> Generate(string text, CancellationToken cancellationToken = default);
}