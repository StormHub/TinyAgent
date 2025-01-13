using System.ComponentModel.DataAnnotations;

namespace TinyAgents.SemanticKernel;

public sealed class OpenAIOptions
{
    [Required] 
    public required string Endpoint { get; init; }
    
    [Required(AllowEmptyStrings = false)] 
    public required string DeploymentName { get; init; }

    [Required(AllowEmptyStrings = false)] 
    public required string ModelId { get; init; }

    public string? ApiKey { get; init; }
}