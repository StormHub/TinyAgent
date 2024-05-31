using System.ComponentModel;
using Azure.Core.GeoJson;
using Microsoft.SemanticKernel;
using TinyAgents.Maps.Azure.Routing;

namespace TinyAgents.SemanticKernel.OpenAI.Plugins;

internal sealed class RoutingPlugin(IRouteApi routeApi)
{
    [KernelFunction(nameof(GetDirections))]
    [Description("Get routing directions between a specified origin and destination in Australia")]
    public async Task<string> GetDirections(
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

        var response = await routeApi.GetRouteDirections(
            new GetRouteDirectionsRequest([origin, destination]), cancellationToken);

        var buffer = new StringBuilder();
        foreach (var route in response.Directions.Routes)
        {
            buffer.AppendLine(
                $" Total length: {route.Summary.LengthInMeters} meters, travel time: {route.Summary.TravelTimeDuration} seconds");

            buffer.AppendLine(" Route path:");
            if (route.Guidance is not null)
                foreach (var instruction in route.Guidance.Instructions)
                    buffer.AppendLine(instruction.Message);
        }

        return buffer.ToString();
    }
}