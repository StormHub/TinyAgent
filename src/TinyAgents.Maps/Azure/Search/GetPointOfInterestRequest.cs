using System.ComponentModel.DataAnnotations;
using Azure.Core.GeoJson;
using Azure.Maps.Search;

namespace TinyAgents.Maps.Azure.Search;

public sealed class GetPointOfInterestRequest(string query, double latitude, double longitude)
{
    public string Query { get; } = query;

    [Required] public GeoPosition Position { get; } = new(longitude, latitude);

    public SearchPointOfInterestOptions GetOptions()
    {
        var options = new SearchPointOfInterestOptions
        {
            Coordinates = Position,
            Top = 5
        };

        return options;
    }
}