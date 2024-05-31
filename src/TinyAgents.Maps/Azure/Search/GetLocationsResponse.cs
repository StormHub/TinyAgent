using Azure.Maps.Search.Models;

namespace TinyAgents.Maps.Azure.Search;

public record DistanceResult(double Kilometers);

public sealed class LocationResult
{
    internal LocationResult(SearchAddressResultItem result)
    {
        Address = result.Address.FreeformAddress;
        Distance = result.DistanceInMeters.HasValue
            ? new DistanceResult(Math.Round(result.DistanceInMeters.Value / 1000, 2))
            : default;
    }

    public string Address { get; }

    public DistanceResult? Distance { get; }
}

public sealed class GetLocationsResponse
{
    internal GetLocationsResponse(IReadOnlyList<SearchAddressResultItem> results)
    {
        Results = results.Select(x => new LocationResult(x)).ToArray();
    }

    public IReadOnlyCollection<LocationResult> Results { get; }
}