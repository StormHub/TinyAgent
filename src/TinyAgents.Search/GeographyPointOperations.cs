using Microsoft.Spatial;

namespace TinyAgents.Search;

internal sealed class GeographyPointOperations : SpatialOperations
{
    private const double EarthRadius = 6378.135; // Kilometers

    public override double Distance(Geography operand1, Geography operand2)
    {
        if (operand1 is not GeographyPoint point1)
            throw new ArgumentException($"{nameof(GeographyPoint)} expected", nameof(operand1));
        if (operand2 is not GeographyPoint point2)
            throw new ArgumentException($"{nameof(GeographyPoint)} expected", nameof(operand2));

        var dLat = ToRadians(point2.Latitude - point1.Latitude);
        var dLon = ToRadians(point2.Longitude - point1.Longitude);

        var a = Math.Pow(Math.Sin(dLat / 2), 2)
                + Math.Pow(Math.Cos(ToRadians(point1.Latitude)), 2) * Math.Pow(Math.Sin(dLon / 2), 2);
        var centralAngle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadius * centralAngle;
    }

    private static double ToRadians(double angle)
    {
        return angle * (Math.PI / 180);
    }
}