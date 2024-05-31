using Azure.Maps.Search.Models;

namespace TinyAgents.Maps.Azure.Search;

public sealed class GetPointOfInterestResponse
{
    internal GetPointOfInterestResponse(IReadOnlyList<SearchAddressResultItem> results)
    {
        Results = results.ToArray();
    }

    public IReadOnlyCollection<SearchAddressResultItem> Results { get; }
}