using System.ComponentModel;
using Azure.Core.GeoJson;
using Microsoft.SemanticKernel;
using TinyAgents.Maps.Azure;

namespace TinyAgents.SemanticKernel.OpenAI.Plugins;

internal sealed class MapPlugin(IMapApi mapApi)
{
    [KernelFunction(nameof(GetPosition))]
    [Description("Get GPS positions for postal address, postcode, suburbs in Australia")]
    public async Task<string> GetPosition(
        [Description("Postal address, postcode, suburbs in Australia to search for")]
        string location)
    {
        var response = await mapApi.GetPositions(new GetPositionsRequest(location));
        var position = response.Positions.Count > 0
            ? response.Positions.First()
            : default(GeoPosition?);

        var buffer = new StringBuilder();
        if (position.HasValue)
        {
            buffer.AppendLine($"latitude: {position.Value.Latitude}");
            buffer.AppendLine($"longitude: {position.Value.Longitude}");
        }
        else
        {
            buffer.AppendLine("Not found");
        }

        return buffer.ToString();
    }

    [KernelFunction(nameof(GetAddress))]
    [Description("Get the address for a given GPS latitude and longitude in Australia")]
    public async Task<string> GetAddress(
        [Description("GPS latitude")] double latitude,
        [Description("GPS longitude")] double longitude)
    {
        var response = await mapApi.GetAddresses(new GetAddressesRequest(latitude, longitude));
        var result = response.Addresses.FirstOrDefault();

        var buffer = new StringBuilder();
        if (result is not null)
        {
            if (!string.IsNullOrEmpty(result.Address.StreetNumber))
                buffer.AppendLine($"street number: {result.Address.StreetNumber}");
            buffer.AppendLine($"street name: {result.Address.StreetName}");

            if (!string.IsNullOrEmpty(result.Address.MunicipalitySubdivision))
                buffer.AppendLine($"suburb: {result.Address.MunicipalitySubdivision}");

            buffer.AppendLine($"state: {result.Address.CountrySubdivision}");
            buffer.AppendLine($"postcode: {result.Address.PostalCode}");
            buffer.AppendLine($"country: {result.Address.Country}");

            buffer.AppendLine($"latitude: {result.Position.Latitude}");
            buffer.AppendLine($"longitude: {result.Position.Longitude}");
        }
        else
        {
            buffer.AppendLine("Not found");
        }

        return buffer.ToString();
    }
}