using Azure.Maps.Search;

namespace TinyAgents.Maps.Azure;

internal sealed class MapApi(MapsSearchClient mapsSearchClient) : IMapApi
{
    public async Task<GetPositionsResponse> GetPositions(GetPositionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = request.GetOptions();
        var response = await mapsSearchClient.SearchAddressAsync(request.Address, options, cancellationToken);
        
        return new GetPositionsResponse(response?.Value?.Results ?? []);
    }

    public async Task<GetAddressesResponse> GetAddresses(GetAddressesRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = request.GetOptions();
        var response = await mapsSearchClient.ReverseSearchAddressAsync(options, cancellationToken);

        return new GetAddressesResponse(response?.Value?.Addresses ?? []);
    }
}