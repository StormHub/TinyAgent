using System.ComponentModel.DataAnnotations;

namespace TinyAgents.SemanticKernel.OpenAI;

public sealed class OpenAIOptions
{
    [Required] public required Uri Uri { get; init; }

    [Required(AllowEmptyStrings = false)] public required string TextGenerationModelId { get; init; }

    [Required] public required string ApiKey { get; init; }

    // Optional for OpenAI
    public string? OrganizationId { get; init; }
}