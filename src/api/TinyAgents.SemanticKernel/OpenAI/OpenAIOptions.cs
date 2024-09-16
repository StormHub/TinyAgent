using System.ComponentModel.DataAnnotations;

namespace TinyAgents.SemanticKernel.OpenAI;

public sealed class OpenAIOptions
{
    [Required] public required Uri Uri { get; init; }

    [Required(AllowEmptyStrings = false)] public required string TextGenerationModelId { get; init; }

    public string? ApiKey { get; init; }
    
    // Optional for OpenAI
    public string? OrganizationId { get; init; }
    
    public TimeSpan RunPollingInterval { get; init; } = TimeSpan.FromSeconds(1);

    public TimeSpan RunPollingBackoff { get; init; } = TimeSpan.FromSeconds(1);

    public int DefaultPollingBackoffThreshold { get; init; } = 1;
}