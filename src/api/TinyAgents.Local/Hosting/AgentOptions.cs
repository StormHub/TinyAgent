using System.ComponentModel.DataAnnotations;

namespace TinyAgents.Local.Hosting;

public sealed class AgentOptions
{
    [Required] public required Uri Uri { get; init; }

    [Required(AllowEmptyStrings = false)] public required string ChannelName { get; init; }
}