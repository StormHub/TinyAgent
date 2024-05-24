using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TinyAgent.Local.Hosting;

IHost? host = default;
try
{
    host = ConsoleHostBuilder.Build(args);

    await host.StartAsync();
    var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

    await using var scope = host.Services.CreateAsyncScope();
    await using var connection = scope.ServiceProvider.GetRequiredService<AgentConnection>();
    await connection.Start(lifetime.ApplicationStopping);

    await host.WaitForShutdownAsync(lifetime.ApplicationStopping);
}
catch (Exception ex)
{
    Console.WriteLine($"Host terminated unexpectedly! \n{ex}");
}
finally
{
    host?.Dispose();
}