using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR.Client;

namespace TinyAgent.Local.Hosting;

internal sealed class AgentConnection : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly AgentOptions _options;

    public AgentConnection(IOptions<AgentOptions> options)
    {
        _options = options.Value;
        var uri = new Uri(_options.Uri, _options.ChannelName);
        _connection = new HubConnectionBuilder()
            .WithUrl(uri)
            .Build();
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        await _connection.StartAsync(cancellationToken);

        foreach (var input in ConsoleInput(cancellationToken))
        {
            var result = _connection.StreamAsync<string>("Streaming", input, cancellationToken: cancellationToken);
            await foreach (var message in result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }
    }

    private static IEnumerable<string> ConsoleInput(CancellationToken cancellationToken)
    {
        yield return "hello"; // Ping first

        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write("User > ");
            var input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                yield return input;
            }
        }
    }

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
}
