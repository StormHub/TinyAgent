using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.Spatial;
using TinyAgents.Search.Azure;

namespace TinyAgents.SemanticKernel.OpenAI.Plugins;

internal sealed class SearchPlugin(ISearchApi searchApi)
{
    [KernelFunction(nameof(GetLocations))]
    [Description(
        "Get electric vehicle charging locations within 100 kilometers radius from a given GPS latitude and longitude in Australia")]
    public async Task<string> GetLocations(
        [Description("GPS latitude")] double latitude,
        [Description("GPS longitude")] double longitude)
    {
        var request = new GetLocationsRequest(latitude, longitude);
        var response = await searchApi.GetLocations(request);

        var buffer = new StringBuilder();
        foreach (var location in response.Locations)
        {
            buffer.Append(location.GetText());

            var distance = location.Point.Distance(request.Point);
            buffer.AppendLine($" kilometers: {distance}\r\n");
        }

        return buffer.ToString();
    }
}