using System.ComponentModel;
using Azure.Core.GeoJson;
using Azure.Maps.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace TinyAgents.Maps;

internal sealed class MapPlugin
{
    private readonly MapsSearchClient _mapsSearchClient;

    private MapPlugin(MapsSearchClient mapsSearchClient)
    {
        _mapsSearchClient = mapsSearchClient;
    }

    [KernelFunction("GetPosition")]
    [Description("Get GPS positions for postal address, postcode, suburbs in Australia")]
    public async Task<string> GetPosition(
        [Description("Postal address, postcode, suburbs in Australia to search for")]
        string location)
    {
        var response = await _mapsSearchClient.SearchAddressAsync(location);
        var results = response?.Value?.Results;
        if (results is null || results.Count <= 0) return "Unknown";

        var buffer = new StringBuilder();
        var position = results[0].Position;

        buffer.AppendLine($"latitude: {position.Latitude}");
        buffer.AppendLine($"longitude: {position.Longitude}");

        return buffer.ToString();
    }

    [KernelFunction("GetAddress")]
    [Description("Get the address for the given GPS latitude and longitude in Australia")]
    public async Task<string> GetAddress(
        [Description("GPS latitude")] double latitude,
        [Description("GPS longitude")] double longitude)
    {
        var options = new ReverseSearchOptions
        {
            Coordinates = new GeoPosition(longitude, latitude)
        };

        var response = await _mapsSearchClient.ReverseSearchAddressAsync(options);
        var address = response.Value.Addresses.Count > 0
            ? response.Value.Addresses[0]
            : default;

        var buffer = new StringBuilder();
        if (address is not null)
        {
            buffer.AppendLine($"latitude, longitude: {address.Position}");
            buffer.AppendLine($"street number: {address.Address.StreetNumber}");
            buffer.AppendLine($"street name: {address.Address.StreetName}");
            buffer.AppendLine($"suburb: {address.Address.MunicipalitySubdivision}");
            buffer.AppendLine($"state: {address.Address.CountrySubdivision}");
            buffer.AppendLine($"postcode: {address.Address.PostalCode}");
            buffer.AppendLine($"country: {address.Address.Country}");
        }
        else
        {
            buffer.AppendLine("Unknown");
        }

        return buffer.ToString();
    }

    public static void ScopeTo(Kernel kernel)
    {
        var mapsSearchClient = kernel.Services.GetRequiredKeyedService<MapsSearchClient>(nameof(MapPlugin));
        var plugin = new MapPlugin(mapsSearchClient);
        kernel.Plugins.AddFromObject(plugin);
    }
}