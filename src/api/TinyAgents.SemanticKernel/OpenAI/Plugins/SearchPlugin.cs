using System.ComponentModel;
using Microsoft.SemanticKernel;
using TinyAgents.Maps.Azure.Search;

namespace TinyAgents.SemanticKernel.OpenAI.Plugins;

internal sealed class SearchPlugin(IMapApi mapApi)
{
    [KernelFunction(nameof(GetLocations))]
    [Description(
        "Get electric vehicle charging locations for given GPS latitude and longitude in Australia")]
    public async Task<GetLocationsResponse> GetLocations(
        [Description("GPS latitude")] double latitude,
        [Description("GPS longitude")] double longitude,
        CancellationToken cancellationToken = default)
    {
        var request = new GetLocationsRequest(latitude, longitude);
        var response = await mapApi.GetLocations(request, cancellationToken);
        return response;
    }
}