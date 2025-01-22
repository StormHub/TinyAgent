using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace TinyAgents.SemanticKernel.Json;

using ChatResponseFormat = OpenAI.Chat.ChatResponseFormat;

internal static class ResponseFormat
{
    public static ChatResponseFormat Json<T>(string? description = default, JsonSerializerOptions? jsonSerializerOptions = default) 
        where T : class
    {
        var inferenceOptions = new AIJsonSchemaCreateOptions
        {
            IncludeSchemaKeyword = false,
            DisallowAdditionalProperties = true,
        };

        var jsonElement = AIJsonUtilities.CreateJsonSchema(
            typeof(T),
            description: description,
            serializerOptions: jsonSerializerOptions,
            inferenceOptions: inferenceOptions);
        var text = jsonElement.GetRawText();
        
        var kernelJsonSchema = KernelJsonSchema.Parse(text);
        var jsonSchemaData = BinaryData.FromObjectAsJson(kernelJsonSchema, jsonSerializerOptions);

        return ChatResponseFormat.CreateJsonSchemaFormat(
            nameof(T).ToLowerInvariant(),
            jsonSchemaData,
            jsonSchemaIsStrict: true);
    }    
    
}