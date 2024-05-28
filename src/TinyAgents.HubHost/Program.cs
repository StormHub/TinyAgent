using TinyAgents.HubHost.Hosting;
using TinyAgents.Search;

IHost? host = default;
try
{
    host = AgentHostBuilder.Build(args);
    
    await using (var scope = host.Services.CreateAsyncScope())
    {
        var indexInitializer = scope.ServiceProvider.GetRequiredService<ISearchIndexInitializer>();
        await indexInitializer.EnsureExists();
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