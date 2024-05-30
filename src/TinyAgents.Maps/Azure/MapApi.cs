using Azure.Maps.Search;

namespace TinyAgents.Maps.Azure;

internal sealed class MapApi(MapsSearchClient mapsSearchClient) : IMapApi
{
    public async Task<GetPositionsResponse> GetPositions(GetPositionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = request.GetOptions();
        var response = await mapsSearchClient.SearchAddressAsync(request.Address, options, cancellationToken);
        var result = response?.Value;

        return new GetPositionsResponse(result?.Results ?? []);
    }

    public async Task<GetAddressesResponse> GetAddresses(GetAddressesRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = request.GetOptions();
        var response = await mapsSearchClient.ReverseSearchAddressAsync(options, cancellationToken);
        var result = response?.Value;

        return new GetAddressesResponse(result?.Addresses ?? []);
    }
}