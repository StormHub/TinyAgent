using Microsoft.Extensions.Options;
using TinyAgents.HubHost.Hosting;
using TinyAgents.Search;
using TinyAgents.SemanticKernel.OpenAI;

IHost? host = default;
try
{
    host = AgentHostBuilder.Build(args);
    
    await using (var scope = host.Services.CreateAsyncScope())
    {
        var options = scope.ServiceProvider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
        await scope.ServiceProvider.EnsureIndexExists(options.TextEmbeddingModelId);
    }
    
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Host terminated unexpectedly! \n{ex}");
}
finally
{
    host?.Dispose();
}