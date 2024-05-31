using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using TinyAgents.Maps.Azure.Search;
using TinyAgents.Shared.Json;

namespace TinyAgents.SemanticKernel.OpenAI.Plugins;

internal sealed class MapPlugin(IMapApi mapApi)
{
    [KernelFunction(nameof(GetPosition))]
    [Description("Get GPS positions for postal address, postcode, suburbs in Australia")]
    public async Task<string?> GetPosition(
        [Description("Postal address, postcode, suburbs in Australia to search for")]
        string location, CancellationToken cancellationToken = default)
    {
        var response = await mapApi.GetPositions(new GetPositionsRequest(location), cancellationToken);
        var result = response.Results.FirstOrDefault();
        return result is not null
            ? JsonSerializer.Serialize(result.Position, DefaultJsonOptions.DefaultSerializerOptions)
            : default;
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