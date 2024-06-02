using System.Runtime.CompilerServices;
using System.Text.Json;
using Azure;
using Azure.Maps.Search.Models;

namespace TinyAgents.Maps.Azure.Search;

public sealed class ChargingConnector
{
    internal ChargingConnector(string type, int ratedPowerInKilowatts, int voltage, string currentType,
        int currentAmpere)
    {
        Type = type;
        RatedPowerInKilowatts = ratedPowerInKilowatts;
        Voltage = voltage;
        CurrentType = currentType;
        CurrentAmpere = currentAmpere;
    }

    public string Type { get; }

    public int RatedPowerInKilowatts { get; }

    public int Voltage { get; }

    public string CurrentType { get; }

    public int CurrentAmpere { get; }

    internal static ChargingConnector? FromJson(JsonElement jsonElement)
    {
        string? type = default;
        int? ratedPowerInKilowatts = default;
        int? voltage = default;
        int? currentAmpere = default;
        string? currentType = default;

        foreach (var connectorProperty in jsonElement.EnumerateObject())
        {
            if (connectorProperty.NameEquals("connectorType"u8))
            {
                type = connectorProperty.Value.GetString();
                continue;
            }

            if (connectorProperty.NameEquals("ratedPowerKW"u8))
            {
                ratedPowerInKilowatts = connectorProperty.Value.GetInt32();
                continue;
            }

            if (connectorProperty.NameEquals("voltageV"u8))
            {
                voltage = connectorProperty.Value.GetInt32();
                continue;
            }

            if (connectorProperty.NameEquals("currentA"u8))
            {
                currentAmpere = connectorProperty.Value.GetInt32();
                continue;
            }

            if (connectorProperty.NameEquals("currentType"u8)) currentType = connectorProperty.Value.GetString();
        }

        return type is not null
               && ratedPowerInKilowatts is not null
               && voltage is not null
               && currentAmpere is not null
               && currentType is not null
            ? new ChargingConnector(
                type,
                ratedPowerInKilowatts.Value,
                voltage.Value,
                currentType,
                currentAmpere.Value)
            : default;
    }
}

internal static class ResponseExtensions
{
    public static async IAsyncEnumerable<ChargingPark> AsEnumerable(this Response<SearchAddressResult> response,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var connectors = await FromJson(
            response.GetRawResponse().Content.ToStream(),
            cancellationToken);

        foreach (var result in response.Value.Results)
        {
            var distance = result.DistanceInMeters.HasValue
                ? Math.Round(result.DistanceInMeters.Value / 1000, 2)
                : default(double?);

            IEnumerable<ChargingConnector>? chargingConnectors = default;
            if (connectors.TryGetValue(result.Id, out var value)) chargingConnectors = value;

            yield return new ChargingPark(
                result.PointOfInterest.Name,
                result.Address.FreeformAddress,
                chargingConnectors ?? [],
                distance);
        }
    }

    private static async Task<IReadOnlyDictionary<string, IReadOnlyCollection<ChargingConnector>>> FromJson(
        Stream stream, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, IReadOnlyCollection<ChargingConnector>>();

        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!property.NameEquals("results"u8)) continue;

            foreach (var item in property.Value.EnumerateArray())
            {
                var entry = GetResultEntry(item);
                if (entry is not null) results.Add(entry.Value.Item1, entry.Value.Item2);
            }
        }

        return results;
    }

    private static (string, IReadOnlyCollection<ChargingConnector>)? GetResultEntry(JsonElement jsonElement)
    {
        string? id = default;
        var connectors = new List<ChargingConnector>();
        foreach (var property in jsonElement.EnumerateObject())
        {
            if (property.NameEquals("id"u8))
            {
                id = property.Value.GetString();
                continue;
            }

            if (property.NameEquals("chargingPark"u8)) connectors.AddRange(FromJson(property.Value));
        }

        return !string.IsNullOrEmpty(id) && connectors.Count > 0
            ? (id, connectors)
            : default((string, IReadOnlyCollection<ChargingConnector>)?);
    }

    private static IEnumerable<ChargingConnector> FromJson(JsonElement jsonElement)
    {
        foreach (var chargingParkProperty in jsonElement.EnumerateObject())
        {
            if (!chargingParkProperty.NameEquals("connectors"u8)) continue;

            foreach (var item in chargingParkProperty.Value.EnumerateArray())
            {
                var connector = ChargingConnector.FromJson(item);
                if (connector is not null) yield return connector;
            }
        }
    }
}

public sealed class ChargingPark
{
    internal ChargingPark(string name, string address, IEnumerable<ChargingConnector> connectors,
        double? distanceInKilometers)
    {
        Name = name;
        Address = address;
        Connectors = connectors.ToArray();
        DistanceInKilometers = distanceInKilometers;
    }

    public string Name { get; }

    public string Address { get; }

    public IReadOnlyCollection<ChargingConnector> Connectors { get; }

    public double? DistanceInKilometers { get; }
}

public sealed class GetLocationsResponse
{
    internal GetLocationsResponse(IEnumerable<ChargingPark> results)
    {
        Results = results.ToArray();
    }

    public IReadOnlyCollection<ChargingPark> Results { get; }
}