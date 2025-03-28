using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

namespace TinyAgents.Plugins.Azure.Search;

public sealed class SearchPlugin
{
    private readonly BingTextSearch _bingTextSearch;
    private readonly ILogger _logger;

    public SearchPlugin(BingTextSearch bingTextSearch, ILoggerFactory loggerFactory)
    {
        _bingTextSearch = bingTextSearch;
        _logger = loggerFactory.CreateLogger<SearchPlugin>();
    }
    
    [KernelFunction]
    [Description("Search the web for the latest information.")]
    public async Task<IReadOnlyCollection<TextSearchResult>> Search(
        [Description("Search query")] string query,
        [Description("Number of results")] int count = 5,
        [Description("Number of results to skip")] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Search web {Query} {Count} {Offset}", query, count, offset);

        var filter = new TextSearchFilter();
        filter.Equality("responseFilter", "Webpages");
        var searchOptions = new TextSearchOptions
        {
            Top = count,
            Skip = offset,
            Filter = filter
        };

        return await Search(query, searchOptions, cancellationToken);
    }

    private async Task<IReadOnlyCollection<TextSearchResult>> Search(
        string query,
        TextSearchOptions searchOptions, 
        CancellationToken cancellationToken = default)
    {
        var response = await _bingTextSearch.GetTextSearchResultsAsync(
            query, 
            searchOptions,
            cancellationToken);
        var results = new List<TextSearchResult>();
        await foreach (var result in response.Results.WithCancellation(cancellationToken))
        {
            results.Add(result);
        }

        return results;
    }
}