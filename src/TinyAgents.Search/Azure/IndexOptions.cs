using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core.Serialization;

namespace TinyAgents.Search.Azure;

public sealed class IndexOptions
{
    [Required]
    public required Uri Uri { get; init; }
    
    [Required(AllowEmptyStrings = false)]
    public required string ApiKey { get; init; }
    
    internal static JsonObjectSerializer JsonObjectSerializer { get; }

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

        JsonObjectSerializer = new (jsonOptions);
    }
}