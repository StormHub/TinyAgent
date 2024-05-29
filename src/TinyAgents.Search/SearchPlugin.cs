using System.ComponentModel;
using Azure.Search.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.Spatial;
using TinyAgents.Search.Azure;

namespace TinyAgents.Search;

internal sealed class SearchPlugin
{
    private readonly ILogger _logger;
    private readonly SearchClient _searchClient;

    static SearchPlugin()
    {
        SpatialImplementation.CurrentImplementation.Operations = new GeographyPointOperations();
    }

    public SearchPlugin(SearchClient searchClient, ILogger<SearchPlugin> logger)
    {
        _searchClient = searchClient;
        _logger = logger;
    }

    [KernelFunction(nameof(GetLocations))]
    [Description("Get electric vehicle charging locations for a given GPS latitude and longitude in Australia")]
    public async Task<string> GetLocations(
        [Description("GPS latitude")] double latitude,
        [Description("GPS longitude")] double longitude)
    {
        var searchOptions = new SearchOptions
        {
            IncludeTotalCount = true,
            Size = 5
        };
        var fieldName = nameof(LocationIndex.Point).ToLowerInvariant();
        var point = GeographyPoint.Create(latitude, longitude);

        searchOptions.Filter = GeographyDistanceInRange(
            point, 100, fieldName);
        searchOptions.OrderBy.Add(DistanceAscending(point, fieldName));

        var response = await _searchClient.SearchAsync<LocationIndex>("*", searchOptions);
        _logger.LogInformation("Location ({latitude}, {longitude}) {TotalCount}",
            latitude, longitude, response.Value.TotalCount);

        var buffer = new StringBuilder();
        await foreach (var result in response.Value.GetResultsAsync())
        {
            buffer.Append(result.Document.GetText());

            var distance = result.Document.Point.Distance(point);
            buffer.AppendLine($" kilometers: {distance}");
        }

        return buffer.ToString();
    }

    private static string DistanceAscending(GeographyPoint point, string fieldName)
    {
        return $"{GeographyDistanceTo(point, fieldName)} asc";
    }

    private static string GeographyDistanceInRange(GeographyPoint point, int kilometers, string fieldName)
    {
        return $"{GeographyDistanceTo(point, fieldName)} le {kilometers}";
    }

    private static string GeographyDistanceTo(GeographyPoint point, string fieldName)
    {
        return $"geo.distance({fieldName}, {GeographyPointOf(point)})";
    }

    private static string GeographyPointOf(GeographyPoint point)
    {
        return $"geography'POINT({point.Longitude} {point.Latitude})'";
    }
}