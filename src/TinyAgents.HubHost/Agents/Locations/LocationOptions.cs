using System.ComponentModel.DataAnnotations;

namespace TinyAgents.HubHost.Agents.Locations;

public sealed class LocationOptions
{
    [Required(AllowEmptyStrings = false)]
    public required string ApiKey { get; init; }
}