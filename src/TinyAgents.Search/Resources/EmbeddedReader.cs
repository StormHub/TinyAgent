using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Spatial;
using TinyAgents.Search.Azure;

namespace TinyAgents.Search.Resources;

internal sealed class EmbeddedReader : IDisposable
{
    private const char Comma = ',';
    private const char Quote = '"';

    private readonly StreamReader _streamReader;

    public EmbeddedReader(Stream stream)
    {
        _streamReader = new StreamReader(stream);
    }

    public void Dispose()
    {
        _streamReader.Dispose();
    }

    public async IAsyncEnumerable<LocationIndex> Read(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // First line is names
        var line = await _streamReader.ReadLineAsync(cancellationToken);
        if (line is null) yield break;

        var names = line.Split(Comma);
        if (!FieldMap.TryCreate(names, out var map)) yield break;

        line = await _streamReader.ReadLineAsync(cancellationToken);
        while (line is not null)
        {
            var data = SplitLine(line).ToArray();

            var name = map.Name(data);
            var latitude = map.Latitude(data);
            var longitude = map.Longitude(data);
            var address = map.Address(data);
            var description = map.Description(data);

            if (!string.IsNullOrEmpty(name)
                && latitude is not null
                && longitude is not null
                && !string.IsNullOrEmpty(address))
            {
                var point = GeographyPoint.Create(latitude.Value, longitude.Value);
                var id = LocationIndex.GenerateId(point);
                yield return new LocationIndex
                {
                    Id = id.ToString(),
                    Name = name,
                    Point = point,
                    Address = address,
                    Description = description
                };
            }

            line = await _streamReader.ReadLineAsync(cancellationToken);
        }
    }

    private static IEnumerable<string> SplitLine(string line)
    {
        var start = 0;

        var index = 0;
        while (index < line.Length)
        {
            if (line[index] == Comma)
            {
                if (line[start] != Quote)
                {
                    if (index == start)
                    {
                        // Empty
                        yield return string.Empty;
                        start++;
                    }
                    else if (index > start)
                    {
                        yield return line.Substring(start, index - start);
                        start = index + 1;
                    }
                }
                else
                {
                    if (line[index - 1] == Quote)
                    {
                        var value = line.Substring(start, index - start);
                        yield return value.Trim(Quote);
                        start = index + 1;
                    }
                }
            }

            index++;
        }

        if (start < line.Length) yield return line[start..].Trim(Quote);
    }

    private sealed class FieldMap
    {
        private readonly int _addressIndex;
        private readonly int _descriptionIndex;
        private readonly int _latitudeIndex;
        private readonly int _longitudeIndex;
        private readonly int _nameIndex;

        private FieldMap(
            int nameIndex,
            int latitudeIndex,
            int longitudeIndex,
            int addressIndex,
            int descriptionIndex)
        {
            _nameIndex = nameIndex;
            _latitudeIndex = latitudeIndex;
            _longitudeIndex = longitudeIndex;
            _addressIndex = addressIndex;
            _descriptionIndex = descriptionIndex;
        }

        public string? Name(string[] data)
        {
            return GetString(data, _nameIndex);
        }

        public double? Latitude(string[] data)
        {
            return GetDouble(data, _latitudeIndex);
        }

        public double? Longitude(string[] data)
        {
            return GetDouble(data, _longitudeIndex);
        }

        public string? Address(string[] data)
        {
            return GetString(data, _addressIndex);
        }

        public string? Description(string[] data)
        {
            return GetString(data, _descriptionIndex);
        }

        private static double? GetDouble(string[] data, int index)
        {
            var token = GetString(data, index);
            return double.TryParse(token, out var value)
                ? value
                : default(double?);
        }

        private static string? GetString(string[] data, int index)
        {
            return data.Length > index ? data[index] : default;
        }

        public static bool TryCreate(string[] names, [NotNullWhen(true)] out FieldMap? map)
        {
            // Location Name,Latitude,Longitude,Address,Description
            map = default;

            var nameIndex = Array.IndexOf<string>(names, "Location Name");
            if (nameIndex < 0) return false;

            var latitudeIndex = Array.IndexOf<string>(names, "Latitude");
            if (latitudeIndex < 0) return false;

            var longitudeIndex = Array.IndexOf<string>(names, "Longitude");
            if (longitudeIndex < 0) return false;

            var addressIndex = Array.IndexOf<string>(names, "Address");
            if (addressIndex < 0) return false;

            var descriptionIndex = Array.IndexOf<string>(names, "Description");
            if (descriptionIndex < 0) return false;

            map = new FieldMap(nameIndex, latitudeIndex, longitudeIndex, addressIndex, descriptionIndex);
            return true;
        }
    }
}