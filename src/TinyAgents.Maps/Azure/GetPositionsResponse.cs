using Azure.Core.GeoJson;
using Azure.Maps.Search.Models;

namespace TinyAgents.Maps.Azure;

public sealed class GetPositionsResponse
{
    internal GetPositionsResponse(IReadOnlyList<SearchAddressResultItem> results)
    {
        Positions = results
            .Select(x => x.Position)
            .ToArray();
    }

    public IReadOnlyCollection<GeoPosition> Positions { get; }
}