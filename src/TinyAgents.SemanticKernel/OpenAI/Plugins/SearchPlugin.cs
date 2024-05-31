using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using TinyAgents.Maps.Azure.Search;
using TinyAgents.Shared.Json;

namespace TinyAgents.SemanticKernel.OpenAI.Plugins;

internal sealed class SearchPlugin(IMapApi mapApi)
{
    [KernelFunction(nameof(GetLocations))]
    [Description(
        "Get electric vehicle charging locations fro a given GPS latitude and longitude in Australia")]
    public async Task<string> GetLocations(
        [Description("GPS latitude")] double latitude,
        [Description("GPS longitude")] double longitude,
        CancellationToken cancellationToken = default)
    {
        var request = new GetLocationsRequest(latitude, longitude);
        var response = await mapApi.GetLocations(request, cancellationToken);

        return JsonSerializer.Serialize(response, DefaultJsonOptions.DefaultSerializerOptions);
    }
}