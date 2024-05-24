using System.ComponentModel;
using Microsoft.SemanticKernel;
using Azure.Maps.Search;

namespace TinyAgents.HubHost.Agents;

internal sealed class LocationPlugin
{
    private readonly MapsSearchClient _mapsSearchClient;

    private LocationPlugin(MapsSearchClient mapsSearchClient)
    {
        _mapsSearchClient = mapsSearchClient;
    }

    [KernelFunction("FindGpsPosition")]
    [Description("Find GPS positions in 'latitude, longitude' string format for postal address, postcode, suburbs in Australia")]
    public async Task<string> FindGpsPosition(
        [Description("Postal address, postcode, suburbs in Australia to search for")] string location)
    {
        var response = await _mapsSearchClient.SearchAddressAsync(location);
        var results = response?.Value?.Results;
        if (results is not null && results.Count > 0)
        {
            var buffer = new StringBuilder();
            var position = results[0].Position;
            var address = results[0].Address;

            buffer.AppendLine($" - address: {address.CountrySubdivisionName} {address.CountrySecondarySubdivision}");
            buffer.AppendLine($" - gps position: {position.Latitude}, {position.Longitude}");

            return buffer.ToString();
        }

        return "Unknown";
    }

    public static Task<LocationPlugin> Create(Kernel kernel)
    {
        var mapsSearchClient = kernel.Services.GetRequiredKeyedService<MapsSearchClient>(nameof(LocationPlugin));
        var plugin = new LocationPlugin(mapsSearchClient);
        return Task.FromResult(plugin);
    }
}
