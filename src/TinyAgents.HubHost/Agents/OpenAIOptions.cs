using System.ComponentModel.DataAnnotations;

namespace TinyAgents.HubHost.Agents;

public sealed class OpenAIOptions
{
    [Required] public required Uri Uri { get; init; }

    [Required(AllowEmptyStrings = false)] public required string ModelId { get; init; }

    public string? ApiKey { get; init; }
}