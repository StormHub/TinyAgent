using TinyAgents.SemanticKernel.Json;

namespace TinyAgents.Hosting;

public record Charger(string Type, string Description);

public sealed class JsonResponse
{
    public static string JsonSchema() => ResponseFormat.JsonSchema<Charger[]>().GetRawText();
}