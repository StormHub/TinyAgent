using TinyAgents.HubHost.Hosting;

IHost? host = default;
try
{
    host = HubHostBuilder.Build(args);
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