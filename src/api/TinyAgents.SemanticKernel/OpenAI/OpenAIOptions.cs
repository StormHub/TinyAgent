using System.ComponentModel.DataAnnotations;

namespace TinyAgents.SemanticKernel.OpenAI;

public sealed class OpenAIOptions
{
    [Required] public required string Uri { get; init; }

    [Required(AllowEmptyStrings = false)] 
    public required string ModelId { get; init; }

    public string? ApiKey { get; init; }
}