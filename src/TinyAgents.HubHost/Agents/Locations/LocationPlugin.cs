using System.ComponentModel;
using Azure.Core.GeoJson;
using Azure.Maps.Search;
using Microsoft.SemanticKernel;

namespace TinyAgents.HubHost.Agents.Locations;

internal sealed class LocationPlugin
{
    private readonly MapsSearchClient _mapsSearchClient;

    private LocationPlugin(MapsSearchClient mapsSearchClient)
    {
        _mapsSearchClient = mapsSearchClient;
    }

    [KernelFunction("GetPosition"),
     Description("Get GPS positions for postal address, postcode, suburbs in Australia")]
    public async Task<string> GetPosition(
        [Description("Postal address, postcode, suburbs in Australia to search for")] 
        string location)
    {
        var response = await _mapsSearchClient.SearchAddressAsync(location);
        var results = response?.Value?.Results;
        if (results is null || results.Count <= 0)
        {
            return "Unknown";
        }
        
        var buffer = new StringBuilder();
        var position = results[0].Position;

        buffer.AppendLine($" - latitude: {position.Latitude}");
        buffer.AppendLine($" - longitude: {position.Longitude}");

        return buffer.ToString();
    }

    [KernelFunction("GetAddress"), 
     Description("Get the address for the given GPS latitude and longitude in Australia")]
    public async Task<string> GetAddress(
        [Description("GPS latitude")] double latitude, 
        [Description("GPS longitude")] double longitude)
    {
        var options = new ReverseSearchOptions
        {
            Coordinates = new GeoPosition(longitude, latitude)
        };
        var response = await _mapsSearchClient.ReverseSearchAddressAsync(options);
        var buffer = new StringBuilder();
        foreach (var address in response.Value.Addresses)
        {
            buffer.AppendLine($" - latitude, longitude: {address.Position}");
            buffer.AppendLine($" - street number: {address.Address.StreetNumber}");
            buffer.AppendLine($" - street name: {address.Address.StreetName}");
            buffer.AppendLine($" - suburb: {address.Address.MunicipalitySubdivision}");
            buffer.AppendLine($" - state: {address.Address.CountrySubdivision}");
            buffer.AppendLine($" - postcode: {address.Address.PostalCode}");
            buffer.AppendLine($" - country: {address.Address.Country}");
        }

        return buffer.ToString();
    }

    public static Task<LocationPlugin> ScopeTo(Kernel kernel)
    {
        var mapsSearchClient = kernel.Services.GetRequiredKeyedService<MapsSearchClient>(nameof(LocationPlugin));
        var plugin = new LocationPlugin(mapsSearchClient);
        kernel.Plugins.AddFromObject(plugin);
        return Task.FromResult(plugin);
    }
}