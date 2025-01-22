using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace TinyAgents.SemanticKernel.Json;

using ChatResponseFormat = OpenAI.Chat.ChatResponseFormat;

public static class ResponseFormat
{
    public static ChatResponseFormat Json<T>(
        string? description = default, 
        AIJsonSchemaCreateOptions? inferenceOptions = default, 
        JsonSerializerOptions? jsonSerializerOptions = default) 
        where T : class
    {
        var text = JsonSchema<T>(description, inferenceOptions, jsonSerializerOptions);
        
        var kernelJsonSchema = KernelJsonSchema.Parse(text);
        var jsonSchemaData = BinaryData.FromObjectAsJson(kernelJsonSchema, jsonSerializerOptions);

        return ChatResponseFormat.CreateJsonSchemaFormat(
            nameof(T).ToLowerInvariant(),
            jsonSchemaData,
            jsonSchemaIsStrict: true);
    }    

    public static string JsonSchema<T>(
        string? description = default,
        AIJsonSchemaCreateOptions? inferenceOptions = default, 
        JsonSerializerOptions? jsonSerializerOptions = default) 
        where T : class
    {
        inferenceOptions ??= new AIJsonSchemaCreateOptions
        {
            IncludeSchemaKeyword = false,
            DisallowAdditionalProperties = true,
        };

        var jsonElement = AIJsonUtilities.CreateJsonSchema(
            typeof(T),
            description: description,
            serializerOptions: jsonSerializerOptions,
            inferenceOptions: inferenceOptions);
        return jsonElement.GetRawText();
    }    
}