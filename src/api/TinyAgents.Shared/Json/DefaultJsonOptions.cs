using System.Text.Json;
using System.Text.Json.Serialization;

namespace TinyAgents.Shared.Json;

public static class DefaultJsonOptions
{
    static DefaultJsonOptions()
    {
        DefaultSerializerOptions = new JsonSerializerOptions();
        DefaultSerializerOptions.Setup();
    }

    public static JsonSerializerOptions DefaultSerializerOptions { get; }

    public static void Setup(this JsonSerializerOptions jsonSerializerOptions)
    {
        jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        jsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}