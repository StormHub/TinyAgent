using System.ComponentModel;
using Microsoft.SemanticKernel;
using TinyAgents.Maps.Azure.Search;

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
        var request = new GetPointOfInterestRequest("electric vehicle station", latitude, longitude);
        var response = await mapApi.GetPointOfInterest(request, cancellationToken);

        var buffer = new StringBuilder();
        foreach (var result in response.Results)
        {
            buffer.AppendLine($"address: {result.Address.FreeformAddress}");
            if (result.DistanceInMeters.HasValue)
                buffer.AppendLine($"kilometers: {Math.Round(result.DistanceInMeters.Value / 1000, 2)}");
            buffer.AppendLine($"name: {result.PointOfInterest.Name}");
        }

        return buffer.ToString();
    }
}