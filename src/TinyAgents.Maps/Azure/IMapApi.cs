namespace TinyAgents.Maps.Azure;

public interface IMapApi
{
    Task<GetPositionsResponse> GetPositions(GetPositionsRequest request, CancellationToken cancellationToken = default);

    Task<GetAddressesResponse> GetAddresses(GetAddressesRequest request, CancellationToken cancellationToken = default);
}