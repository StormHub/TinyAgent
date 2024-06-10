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
        var request = new GetLocationsRequest(latitude, longitude);
        var response = await mapApi.GetLocations(request, cancellationToken);

        var buffer = new StringBuilder();
        foreach (var result in response.Results)
        {
            buffer.AppendLine($" name: {result.Name}");
            buffer.AppendLine($" address: {result.Address}");
            buffer.AppendLine($" distance: {result.DistanceInKilometers} kilometers");
            foreach (var connector in result.Connectors)
            {
                buffer.Append($" connector type: {connector.Type}");
                buffer.Append($" power type: {connector.CurrentType}");
                buffer.Append($" rated power: {connector.RatedPowerInKilowatts} kilowatts");
                buffer.Append($" voltage: {connector.Voltage}");
                buffer.AppendLine($" current: {connector.CurrentAmpere} amp");
            }
        }

        return buffer.ToString();
    }
}