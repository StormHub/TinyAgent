using Azure.Maps.Routing;

namespace TinyAgents.Maps.Azure.Routing;

internal sealed class RouteApi(MapsRoutingClient mapsRoutingClient) : IRouteApi
{
    public async Task<GetRouteDirectionsResponse> GetRouteDirections(
        GetRouteDirectionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = request.GetOptions();
        var query = new RouteDirectionQuery(request.RoutePoints.ToList(), options);
        var response = await mapsRoutingClient.GetDirectionsAsync(query, cancellationToken);
        return new GetRouteDirectionsResponse(response.Value);
    }
}