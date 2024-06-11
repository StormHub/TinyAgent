namespace TinyAgents.Maps.Azure.Routing;

public interface IRouteApi
{
    Task<GetRouteDirectionsResponse> GetRouteDirections(
        GetRouteDirectionsRequest request,
        CancellationToken cancellationToken = default);
}