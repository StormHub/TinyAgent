using System.ComponentModel;
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
}