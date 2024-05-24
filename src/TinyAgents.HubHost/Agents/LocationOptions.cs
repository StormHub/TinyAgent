using System.ComponentModel.DataAnnotations;

namespace TinyAgents.HubHost.Agents;

public sealed class LocationOptions
{
    [Required(AllowEmptyStrings = false)]
    public required string ApiKey { get; init; }
}
