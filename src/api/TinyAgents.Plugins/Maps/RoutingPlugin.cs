using System.ComponentModel;
using Azure.Core.GeoJson;
using Azure.Maps.Routing;
using Azure.Maps.Routing.Models;
using Microsoft.SemanticKernel;

namespace TinyAgents.Plugins.Maps;

public sealed class RoutingPlugin(MapsRoutingClient mapsRoutingClient)
{
    [KernelFunction(nameof(GetRouteDirection))]
    [Description("Get GPS route directions for a given GPS origin and GPS destination")]
    public async Task<IReadOnlyCollection<RouteInstructionGroup>> GetRouteDirection(
        [Description("The original GPS latitude and longitude to route from")] GeoPosition origin, 
        [Description("The destination GPS latitude and longitude to route to")] GeoPosition destination,
        CancellationToken cancellationToken = default)
    {
        var options = new RouteDirectionOptions
        {
            RouteType = RouteType.Fastest,
            TravelMode = TravelMode.Car,
            InstructionsType = RouteInstructionsType.Text
        };
        
        var query = new RouteDirectionQuery([ origin, destination ], options);
        var response = await mapsRoutingClient.GetDirectionsAsync(query, cancellationToken);
        
        var routes = new List<RouteInstructionGroup>();
        foreach (var routeData in response.Value.Routes)
        {
            routes.AddRange(routeData.Guidance.InstructionGroups);
        }

        return routes;
    }
}