using Azure.Core.GeoJson;
using Azure.Maps.Search.Models;

namespace TinyAgents.Maps.Azure.Search;

public record AddressRecord(GeoPosition Position, MapsAddress Address);

public sealed class GetAddressesResponse
{
    internal GetAddressesResponse(IReadOnlyList<ReverseSearchAddressItem> addresses)
    {
        Addresses = AsEnumerable(addresses).ToArray();
    }

    public IReadOnlyCollection<AddressRecord> Addresses { get; }

    private static IEnumerable<AddressRecord> AsEnumerable(IReadOnlyList<ReverseSearchAddressItem> addresses)
    {
        foreach (var address in addresses)
        {
            // "{latitude},{longitude}"
            var position = address.Position.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (position.Length != 2
                || !double.TryParse(position[0], out var latitude)
                || !double.TryParse(position[1], out var longitude))
                throw new ArgumentException($"Invalid geo location {address.Position}", nameof(address));

            yield return new AddressRecord(new GeoPosition(longitude, latitude), address.Address);
        }
    }
}