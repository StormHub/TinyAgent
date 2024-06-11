using System.ComponentModel;
using Microsoft.SemanticKernel;
using TinyAgents.Maps.Azure.Search;

namespace TinyAgents.SemanticKernel.OpenAI.Plugins;

internal sealed class MapPlugin(IMapApi mapApi)
{
    [KernelFunction(nameof(GetPosition))]
    [Description("Get GPS latitude and longitude for a given postal address, postcode, suburbs in Australia")]
    public async Task<string?> GetPosition(
        [Description("Postal address, postcode, suburbs in Australia to search for")]
        string location,
        CancellationToken cancellationToken = default)
    {
        var response = await mapApi.GetPositions(new GetPositionsRequest(location), cancellationToken);
        var result = response.Results.FirstOrDefault();
        if (result is null) return default;

        var buffer = new StringBuilder();
        buffer.Append($" latitude: {result.Position.Latitude}");
        buffer.Append($" longitude: {result.Position.Longitude}");
        return buffer.ToString();
    }

    [KernelFunction(nameof(GetAddress))]
    [Description("Get the address for a given GPS latitude and longitude in Australia")]
    public async Task<string?> GetAddress(
        [Description("GPS latitude")] double latitude,
        [Description("GPS longitude")] double longitude,
        CancellationToken cancellationToken = default)
    {
        var response = await mapApi.GetAddresses(new GetAddressesRequest(latitude, longitude), cancellationToken);
        var result = response.Addresses.FirstOrDefault();
        return result?.Address.FreeformAddress;
    }
}