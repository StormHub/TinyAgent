using Azure.Maps.Search.Models;

namespace TinyAgents.Maps.Azure.Search;

public sealed class GetPositionsResponse
{
    internal GetPositionsResponse(IReadOnlyList<SearchAddressResultItem> results)
    {
        Results = results.ToArray();
    }

    public IReadOnlyCollection<SearchAddressResultItem> Results { get; }
}