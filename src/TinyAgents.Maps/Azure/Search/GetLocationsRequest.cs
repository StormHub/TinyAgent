using System.ComponentModel.DataAnnotations;
using Azure.Core.GeoJson;
using Azure.Maps.Search;

namespace TinyAgents.Maps.Azure.Search;

public sealed class GetLocationsRequest(double latitude, double longitude)
{
    private const int DefaultMaximumResultCount = 5;
    
    [Required] public GeoPosition Position { get; } = new(longitude, latitude);

    public SearchPointOfInterestOptions GetOptions()
    {
        var options = new SearchPointOfInterestOptions
        {
            Coordinates = Position,
            Top = DefaultMaximumResultCount
        };

        return options;
    }
}