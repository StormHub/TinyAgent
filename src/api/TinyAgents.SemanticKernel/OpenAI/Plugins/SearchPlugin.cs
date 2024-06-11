using System.ComponentModel;
using Microsoft.SemanticKernel;
using TinyAgents.Maps.Azure.Search;

namespace TinyAgents.SemanticKernel.OpenAI.Plugins;

internal sealed class SearchPlugin(IMapApi mapApi)
{
    [KernelFunction(nameof(GetLocations))]
    [Description(
        "Get electric vehicle charging locations for a given GPS latitude and longitude in Australia")]
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
            buffer.Append($" name: {result.Name}");
            buffer.Append($" address: {result.Address}");
            
            // https://google.com/maps/?q=<lat>,<long>
            var uri = Uri.EscapeDataString($"https://google.com/maps/?q={result.Position.Latitude},{result.Position.Longitude}");
            buffer.Append($" map: {uri}");

            buffer.Append($" distance: {result.DistanceInKilometers} kilometers");
            foreach (var connector in result.Connectors)
            {
                buffer.Append($" connector type: {connector.Type}");
                buffer.Append($" power type: {connector.CurrentType}");
                buffer.Append($" rated power: {connector.RatedPowerInKilowatts} kilowatts");
                buffer.Append($" voltage: {connector.Voltage}");
                buffer.Append($" current: {connector.CurrentAmpere} amp");
            }

            buffer.AppendLine();
        }

        return buffer.ToString();
    }
}