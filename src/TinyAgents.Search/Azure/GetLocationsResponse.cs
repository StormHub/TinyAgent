namespace TinyAgents.Search.Azure;

public sealed class GetLocationsResponse
{
    internal GetLocationsResponse(IEnumerable<LocationIndex> locations)
    {
        Locations = locations.ToArray();
    }

    public IReadOnlyCollection<LocationIndex> Locations { get; }
}