using System.ComponentModel.DataAnnotations;

namespace TinyAgents.SemanticKernel.AzureAI;

public sealed class AzureConfiguration
{
    [Required] public required string Endpoint { get; init; }

    public string? ApiKey { get; init; }

    [Required(AllowEmptyStrings = false)] public required string DeploymentName { get; init; }

    [Required(AllowEmptyStrings = false)] public required string ModelId { get; init; }
}