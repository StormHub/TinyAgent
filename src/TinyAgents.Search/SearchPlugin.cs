using System.ComponentModel;
using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using TinyAgents.Search.Azure;

namespace TinyAgents.Search;

internal sealed class SearchPlugin
{
    private readonly SearchIndexClient _indexClient;
    private readonly string _indexName;
    private readonly ILogger _logger;

    public SearchPlugin(SearchIndexClient indexClient, IOptions<IndexOptions> options, ILogger<SearchPlugin> logger)
    {
        _indexClient = indexClient;
        _indexName = options.Value.Name.ToLowerInvariant();
        _logger = logger;
    }

    [KernelFunction(nameof(GetLocations))]
    [Description("Get electric vehicle charging locations for a given GPS latitude and longitude in Australia")]
    public Task<string> GetLocations(
        [Description("GPS latitude")] double latitude,
        [Description("GPS longitude")] double longitude)
    {
        return Task.FromResult("Unknown"); // TODO: Search index
    }

    internal async Task EnsureExists(CancellationToken cancellationToken = default)
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
    }
}