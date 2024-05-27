using System.ComponentModel.DataAnnotations;

namespace TinyAgents.SemanticKernel.Assistants;

public sealed class AssistantOptions
{
    [Required] public required Uri Uri { get; init; }

    [Required(AllowEmptyStrings = false)] 
    public required string ModelId { get; init; }
    
    [Required]  
    public required string ApiKey { get; init; }
}