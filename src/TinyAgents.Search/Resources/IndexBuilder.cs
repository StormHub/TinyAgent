using System.Runtime.CompilerServices;
using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using TinyAgents.Search.Azure;

namespace TinyAgents.Search.Resources;

internal sealed class IndexBuilder
{
    private readonly SearchIndexClient _indexClient;
    private readonly string _indexName;
    private readonly IKernelBuilder _kernelBuilder;
    private readonly ILogger _logger;

    public IndexBuilder(
        SearchIndexClient indexClient,
        IOptions<IndexOptions> options,
        IKernelBuilder kernelBuilder,
        ILogger<IndexBuilder> logger)
    {
        _indexClient = indexClient;
        _indexName = options.Value.Name.ToLowerInvariant();
        _kernelBuilder = kernelBuilder;
        _logger = logger;
    }

    internal async Task EnsureExists(string textEmbeddingModelId, CancellationToken cancellationToken = default)
    {
        await foreach (var name in _indexClient.GetIndexNamesAsync(cancellationToken))
            if (string.Equals(name, _indexName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Index {Name} already exists", _indexName);
                return;
            }

        try
        {
            _logger.LogInformation("Creating index {Name}", _indexName);
            var index = LocationIndex.Index(_indexName, IndexOptions.JsonObjectSerializer);
            await _indexClient.CreateIndexAsync(index, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 409) // Already exists
        {
            _logger.LogWarning("Index {Name} already exists {Message}", _indexName, ex.Message);
        }

        var kernel = _kernelBuilder.Build();
        var searchClient = _indexClient.GetSearchClient(_indexName);
        await foreach (var index in LoadEmbeddedResources(cancellationToken))
        {
            await GenerateEmbedding(index, kernel, textEmbeddingModelId, cancellationToken);
            await searchClient.UploadDocumentsAsync([index], cancellationToken: cancellationToken);
        }
    }

    private static async Task GenerateEmbedding(LocationIndex index, Kernel kernel, string textEmbeddingModelId,
        CancellationToken cancellationToken)
    {
        var generator = kernel.Services.GetRequiredKeyedService<ITextEmbeddingGenerationService>(textEmbeddingModelId);
        var text = index.GetEmbeddingText();
        var embedding = await generator.GenerateEmbeddingAsync(text, kernel, cancellationToken);
        index.Embedding = embedding.ToArray();
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