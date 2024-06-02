using Azure.Maps.Search;

namespace TinyAgents.Maps.Azure.Search;

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

    public async Task<GetLocationsResponse> GetLocations(GetLocationsRequest request,
        CancellationToken cancellationToken = default)
    {
        const string electricVehicleStation = "electric vehicle station";

        var options = request.GetOptions();
        var response = await mapsSearchClient.SearchPointOfInterestAsync(
            electricVehicleStation,
            options: options,
            cancellationToken: cancellationToken);

        var results = new List<ChargingPark>();
        await foreach (var result in response.AsEnumerable(cancellationToken)) results.Add(result);

        return new GetLocationsResponse(results);
    }
}