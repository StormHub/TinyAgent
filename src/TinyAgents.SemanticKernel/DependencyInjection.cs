using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TinyAgents.Maps;
using TinyAgents.Search;
using TinyAgents.SemanticKernel.Assistants;
using TinyAgents.SemanticKernel.OpenAI;

namespace TinyAgents.SemanticKernel;

public static class DependencyInjection
{
    public static IServiceCollection AddAssistanceAgent(this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddMaps();
        services.AddSearch();
        services.AddOpenAI(environment);
        services.AddAssistant(configuration);

        return services;
    }

    public static async Task EnsureIndexExists(this AsyncServiceScope scope)
    {
        var options = scope.ServiceProvider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
        await scope.ServiceProvider.EnsureIndexExists(options.TextEmbeddingModelId);
    }
}