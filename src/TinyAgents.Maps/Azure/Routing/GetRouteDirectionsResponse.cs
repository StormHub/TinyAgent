using Azure.Maps.Routing.Models;

namespace TinyAgents.Maps.Azure.Routing;

public sealed class GetRouteDirectionsResponse
{
    internal GetRouteDirectionsResponse(RouteDirections directions)
    {
        Directions = directions;
    }

    public RouteDirections Directions { get; }
}