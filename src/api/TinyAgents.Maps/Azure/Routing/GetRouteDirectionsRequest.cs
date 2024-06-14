using Azure.Core.GeoJson;
using Azure.Maps.Routing;

namespace TinyAgents.Maps.Azure.Routing;

public sealed class GetRouteDirectionsRequest(
    IReadOnlyCollection<GeoPosition> routePoints,
    bool useTrafficInformation = false,
    bool useTextInstructions = true)
{
    public IReadOnlyCollection<GeoPosition> RoutePoints { get; } = routePoints;

    private bool UseTrafficInformation { get; } = useTrafficInformation;

    private bool UseTextInstructions { get; } = useTextInstructions;

    internal RouteDirectionOptions GetOptions()
    {
        var options = new RouteDirectionOptions
        {
            RouteType = RouteType.Shortest,
            TravelMode = TravelMode.Car,
            UseTrafficData = UseTrafficInformation
        };
        if (UseTextInstructions)
        {
            options.InstructionsType = RouteInstructionsType.Text;
        }

        return options;
    }
}