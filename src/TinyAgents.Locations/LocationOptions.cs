using System.ComponentModel.DataAnnotations;

namespace TinyAgents.Locations;

public sealed class LocationOptions
{
    [Required(AllowEmptyStrings = false)] public required string ApiKey { get; init; }
}