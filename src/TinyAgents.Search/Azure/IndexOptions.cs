using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core.Serialization;

namespace TinyAgents.Search.Azure;

public sealed class IndexOptions
{
    static IndexOptions()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        jsonOptions.Converters.Add(new MicrosoftSpatialGeoJsonConverter());

        JsonObjectSerializer = new(jsonOptions);
    }

    [Required] public required Uri Uri { get; init; }

    [Required(AllowEmptyStrings = false)] public required string ApiKey { get; init; }

    public string Name { get; init; } = "locations-index";

    internal static JsonObjectSerializer JsonObjectSerializer { get; }
}