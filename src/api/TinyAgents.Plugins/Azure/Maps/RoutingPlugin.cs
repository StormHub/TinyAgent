using System.ComponentModel;
using Azure.Maps.Routing;
using Azure.Maps.Routing.Models;
using Microsoft.SemanticKernel;

namespace TinyAgents.Plugins.Azure.Maps;

public sealed class RoutingPlugin(MapsRoutingClient mapsRoutingClient)
{
    [KernelFunction(nameof(GetRouteDirection))]
    [Description("Get GPS route directions for a given GPS origin and destination")]
    public async Task<IReadOnlyCollection<RouteInstructionGroup>> GetRouteDirection(
        [Description("The origin GPS latitude and longitude to route from")]
        GeographyPoint origin,
        [Description("The destination GPS latitude and longitude to route to")]
        GeographyPoint destination,
        CancellationToken cancellationToken = default)
    {
        var options = new RouteDirectionOptions
        {
            RouteType = RouteType.Fastest,
            TravelMode = TravelMode.Car,
            InstructionsType = RouteInstructionsType.Text
        };

        var query = new RouteDirectionQuery(
            [origin.AsGeoPosition(), destination.AsGeoPosition()],
            options);
        var response = await mapsRoutingClient.GetDirectionsAsync(query, cancellationToken);

        var routes = new List<RouteInstructionGroup>();
        foreach (var routeData in response.Value.Routes) routes.AddRange(routeData.Guidance.InstructionGroups);

        return routes;
    }
}