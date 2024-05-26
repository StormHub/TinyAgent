using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace TinyAgents.HubHost.Agents.OpenAI;

internal static class DependencyInjection
{
    public static IServiceCollection AddOpenAI(this IServiceCollection services, Action<IKernelBuilder, IServiceProvider>? configureKernel = default)
    {
        services.AddOptions<OpenAIOptions>()
            .BindConfiguration(nameof(OpenAIOptions))
            .ValidateDataAnnotations();

        services.AddHttpClient(nameof(OpenAIClient));

        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(OpenAIClient));
            
            var openAIOptions = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            
            var builder = Kernel.CreateBuilder();
            if (openAIOptions.Uri.Host.EndsWith("openai.azure.com"))
            {
                builder.AddAzureOpenAIChatCompletion(
                    openAIOptions.ModelId,
                    openAIOptions.Uri.ToString(),
                    openAIOptions.ApiKey,
                    openAIOptions.ModelId,
                    openAIOptions.ModelId,
                    httpClient: httpClient);
            }
            else
            {
                builder.AddOpenAIChatCompletion(
                    openAIOptions.ModelId,
                    apiKey: openAIOptions.ApiKey,
                    endpoint: openAIOptions.Uri, 
                    httpClient: httpClient);
            }

            builder.Services.AddKeyedSingleton(nameof(OpenAIClient), httpClient);
            builder.Services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());
            
            configureKernel?.Invoke(builder, provider);

            return builder;
        });
        
        return services;
    }   
}