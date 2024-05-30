using Azure.Search.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Spatial;

namespace TinyAgents.Search.Azure;

internal sealed class SearchApi(SearchClient searchClient, ILogger<SearchApi> logger) : ISearchApi
{
    private readonly ILogger _logger = logger;

    static SearchApi()
    {
        SpatialImplementation.CurrentImplementation.Operations = new GeographyPointOperations();
    }

    public async Task<GetLocationsResponse> GetLocations(GetLocationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = request.GetOptions();
        var response = await searchClient.SearchAsync<LocationIndex>("*", options, cancellationToken);

        _logger.LogInformation("Location ({latitude}, {longitude}) {TotalCount}",
            request.Point.Latitude, request.Point.Longitude, response.Value.TotalCount);

        var results = new List<LocationIndex>();
        await foreach (var result in response.Value.GetResultsAsync().WithCancellation(cancellationToken))
            results.Add(result.Document);

        return new GetLocationsResponse(results);
    }
}