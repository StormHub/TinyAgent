using Azure.Core.GeoJson;

namespace TinyAgents.Plugins.Azure.Maps;

public sealed class GeographyPoint
{
    public required double Longitude { get; init; }

    public required double Latitude { get; init; }

    public static GeographyPoint FromGeoPoint(GeoPoint Geometry)
    {
        return new GeographyPoint
        {
            Longitude = Geometry.Coordinates.Longitude,
            Latitude = Geometry.Coordinates.Latitude
        };
    }

    public GeoPosition AsGeoPosition()
    {
        return new GeoPosition(Longitude, Latitude);
    }
}