using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;

namespace TinyAgents.Plugins.Search;

public sealed class SearchPlugin
{
    private readonly IWebSearchEngineConnector _connector;
    private readonly ILogger _logger;

    public SearchPlugin(IWebSearchEngineConnector connector, ILoggerFactory loggerFactory)
    {
        _connector = connector;
        _logger = loggerFactory.CreateLogger<SearchPlugin>();
    }
    
    [KernelFunction]
    [Description("Perform a web search.")]
    public async Task<IReadOnlyCollection<WebPage>> Search(
        [Description("Text to search for")] string query,
        [Description("Number of results")] int count = 5,
        [Description("Number of results to skip")] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Search web {Query} {Count} {Offset}", query, count, offset);
        var results = await _connector.SearchAsync<WebPage>(query, count, offset, cancellationToken);
        return results.ToArray();
    }
}