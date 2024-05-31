using Azure.Core.GeoJson;
using Azure.Maps.Routing;

namespace TinyAgents.Maps.Azure.Routing;

public sealed class GetRouteDirectionsRequest(
    IReadOnlyCollection<GeoPosition> routePoints,
    bool useTrafficInformation = false)
{
    public IReadOnlyCollection<GeoPosition> RoutePoints { get; } = routePoints;

    private bool UseTrafficInformation { get; } = useTrafficInformation;

    internal RouteDirectionOptions GetOptions()
    {
        var options = new RouteDirectionOptions
        {
            RouteType = RouteType.Fastest,
            TravelMode = TravelMode.Car,
            UseTrafficData = UseTrafficInformation,
            InstructionsType = RouteInstructionsType.Text
        };

        return options;
    }
}