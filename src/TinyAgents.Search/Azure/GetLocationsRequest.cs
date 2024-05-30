using Azure.Search.Documents;
using Microsoft.Spatial;

namespace TinyAgents.Search.Azure;

public sealed class GetLocationsRequest(double latitude, double longitude)
{
    public GeographyPoint Point { get; } = GeographyPoint.Create(latitude, longitude);

    public SearchOptions GetOptions()
    {
        var searchOptions = new SearchOptions
        {
            IncludeTotalCount = true,
            Size = IndexOptions.MaximumResultCount
        };

        var fieldName = nameof(LocationIndex.Point).ToLowerInvariant();
        searchOptions.Filter = GeographyDistanceInRange(
            Point,
            IndexOptions.MaximumDistanceInKilometers,
            fieldName);
        searchOptions.OrderBy.Add(DistanceAscending(Point, fieldName));

        return searchOptions;
    }

    private static string DistanceAscending(GeographyPoint point, string fieldName)
    {
        return $"{GeographyDistanceTo(point, fieldName)} asc";
    }

    private static string GeographyDistanceInRange(GeographyPoint point, int kilometers, string fieldName)
    {
        return $"{GeographyDistanceTo(point, fieldName)} le {kilometers}";
    }

    private static string GeographyDistanceTo(GeographyPoint point, string fieldName)
    {
        return $"geo.distance({fieldName}, {GeographyPointOf(point)})";
    }

    private static string GeographyPointOf(GeographyPoint point)
    {
        return $"geography'POINT({point.Longitude} {point.Latitude})'";
    }
}