using System.ComponentModel;
using Azure.Core.GeoJson;
using Azure.Maps.Routing.Models;
using Microsoft.SemanticKernel;
using TinyAgents.Maps.Azure.Routing;

namespace TinyAgents.SemanticKernel.OpenAI.Plugins;

internal sealed class RoutingPlugin(IRouteApi routeApi)
{
    [KernelFunction(nameof(GetDirections))]
    [Description("Get route directions in GPS coordinats between a specified origin and destination in Australia")]
    public async Task<RouteData?> GetDirections(
        [Description("Origin GPS latitude")] double originLatitude,
        [Description("Origin GPS longitude")] double originLongitude,
        [Description("Destination GPS latitude")]
        double destinationLatitude,
        [Description("Destination GPS longitude")]
        double destinationLongitude,
        CancellationToken cancellationToken = default)
    {
        var origin = new GeoPosition(originLongitude, originLatitude);
        var destination = new GeoPosition(destinationLongitude, destinationLatitude);

        var request = new GetRouteDirectionsRequest([origin, destination], useTextInstructions: false);
        var response = await routeApi.GetRouteDirections(request, cancellationToken);
        var routes = response.Directions.Routes;
        
        return routes.Count > 0 ? routes[0] : default;
    }
}