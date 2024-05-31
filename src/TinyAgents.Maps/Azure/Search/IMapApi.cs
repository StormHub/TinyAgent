namespace TinyAgents.Maps.Azure.Search;

public interface IMapApi
{
    Task<GetPositionsResponse> GetPositions(GetPositionsRequest request, CancellationToken cancellationToken = default);

    Task<GetAddressesResponse> GetAddresses(GetAddressesRequest request, CancellationToken cancellationToken = default);

    Task<GetPointOfInterestResponse> GetPointOfInterest(GetPointOfInterestRequest request,
        CancellationToken cancellationToken = default);
}