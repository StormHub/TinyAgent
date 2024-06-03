using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace TinyAgent.Local.Hosting;

internal sealed class AgentConnection : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly ILogger _logger;

    public AgentConnection(IOptions<AgentOptions> options, ILogger<AgentConnection> logger)
    {
        var options1 = options.Value;
        var uri = new Uri(options1.Uri, options1.ChannelName);
        _connection = new HubConnectionBuilder()
            .WithUrl(uri)
            .Build();
        _logger = logger;
    }

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();

    public async Task Start(CancellationToken cancellationToken)
    {
        await _connection.StartAsync(cancellationToken);

        foreach (var input in ConsoleInput(cancellationToken))
        {
            try
            {
                await foreach (var message in GetMessages(input, cancellationToken))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(message.Content);
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get messages");
                throw;
            }
        }
    }

    private async IAsyncEnumerable<ChatMessageContent> GetMessages(string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var message in
                       _connection.StreamAsync<ChatMessageContent>("Streaming", input, cancellationToken))
        {
            if (!string.IsNullOrEmpty(message.Content))
            {
                yield return message;
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
            if (!string.IsNullOrEmpty(input)) yield return input;
        }
    }
}