using System.ComponentModel.DataAnnotations;

namespace TinyAgents.SemanticKernel.OpenAI;

public sealed class OpenAIOptions
{
    [Required] public required Uri Uri { get; init; }

    [Required(AllowEmptyStrings = false)] public required string ModelId { get; init; }

    [Required] public required string ApiKey { get; init; }
}