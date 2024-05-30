using System.Runtime.CompilerServices;
using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TinyAgents.Search.Azure;

namespace TinyAgents.Search.Resources;

internal sealed class IndexBuilder(SearchIndexClient indexClient, IOptions<IndexOptions> options, ILogger<IndexBuilder> logger)
{
    private readonly string _indexName = options.Value.Name.ToLowerInvariant();
    private readonly ILogger _logger = logger;

    internal async Task EnsureExists(ITextEmbedding textEmbedding, CancellationToken cancellationToken = default)
    {
        await foreach (var name in indexClient.GetIndexNamesAsync(cancellationToken))
            if (string.Equals(name, _indexName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Index {Name} already exists", _indexName);
                return;
            }

        try
        {
            _logger.LogInformation("Creating index {Name}", _indexName);
            var index = LocationIndex.Index(_indexName, IndexOptions.JsonObjectSerializer);
            await indexClient.CreateIndexAsync(index, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 409) // Already exists
        {
            _logger.LogWarning("Index {Name} already exists {Message}", _indexName, ex.Message);
        }

        var searchClient = indexClient.GetSearchClient(_indexName);
        await foreach (var index in LoadEmbeddedResources(cancellationToken))
        {
            var text = index.GetText();
            var embedding = await textEmbedding.Generate(text, cancellationToken);
            index.Embedding = embedding.ToArray();
            await searchClient.UploadDocumentsAsync([index], cancellationToken: cancellationToken);
        }
    }

    private async IAsyncEnumerable<LocationIndex> LoadEmbeddedResources(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var assembly = typeof(IndexBuilder).Assembly;
        foreach (var name in assembly
                     .GetManifestResourceNames()
                     .Where(x => string.Equals(Path.GetExtension(x), ".csv", StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogInformation("Loading {Name}", name);

            await using var stream = assembly.GetManifestResourceStream(name)
                                     ?? throw new InvalidOperationException($"Unable to load resource {name}");
            using var reader = new EmbeddedReader(stream);
            await foreach (var index in reader.Read(cancellationToken)) yield return index;
        }
    }
}