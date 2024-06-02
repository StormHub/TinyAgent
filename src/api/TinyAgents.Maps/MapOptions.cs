using System.ComponentModel.DataAnnotations;

namespace TinyAgents.Maps;

public sealed class MapOptions
{
    [Required(AllowEmptyStrings = false)] public required string ApiKey { get; init; }
}