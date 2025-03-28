using Azure.Core.GeoJson;

namespace TinyAgents.Plugins.Azure.Maps;

public sealed class GeographyPoint
{
    public required double Longitude { get; init; }

    public required double Latitude { get; init; }

    public static GeographyPoint FromGeoPoint(GeoPoint Geometry) =>
        new()
        {
            Longitude = Geometry.Coordinates.Longitude,
            Latitude = Geometry.Coordinates.Latitude
        };
    
    public GeoPosition AsGeoPosition() => new(longitude: Longitude, latitude: Latitude);
}