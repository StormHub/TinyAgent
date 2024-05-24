using System.ComponentModel.DataAnnotations;

namespace TinyAgents.HubHost.Agents;

public sealed class OpenAIOptions
{
    [Required]
    public required Uri Uri { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string EmbeddingsDeploymentName { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string TextGenerationDeploymentName { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string ApiKey { get; init; }
}