using System.ComponentModel.DataAnnotations;
using Azure.Core.GeoJson;
using Azure.Maps.Search;

namespace TinyAgents.Maps.Azure;

public sealed class GetAddressesRequest(double latitude, double longitude)
{
    [Required] public GeoPosition Position { get; } = new(longitude, latitude);

    internal ReverseSearchOptions GetOptions()
    {
        var options = new ReverseSearchOptions
        {
            Coordinates = Position,
            AllowFreeformNewline = false
        };

        return options;
    }
}