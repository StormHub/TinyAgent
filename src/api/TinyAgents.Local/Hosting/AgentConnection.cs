﻿using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using TinyAgents.Shared.Json;

namespace TinyAgents.Local.Hosting;

internal sealed class AgentConnection : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly ILogger _logger;

    public AgentConnection(IOptions<AgentOptions> options, ILogger<AgentConnection> logger)
    {
        var agentOptions = options.Value;

        var builder = new UriBuilder(agentOptions.Uri)
        {
            Path = agentOptions.ChannelName,
        };
        var uri = builder.ToString();
        _connection = new HubConnectionBuilder()
            .AddJsonProtocol(jsonOptions =>
            {
                jsonOptions.PayloadSerializerOptions.Setup();
            })
            .WithUrl(uri)
            .Build();
        _logger = logger;
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        await _connection.StartAsync(cancellationToken);

        foreach (var input in ConsoleInput(cancellationToken))
            try
            {
                await foreach (var message in GetMessages(input, cancellationToken))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{message.Role} > {message.Content}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get messages");
                throw;
            }
    }

    private async IAsyncEnumerable<ChatMessageContent> GetMessages(
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var message in
                       _connection.StreamAsync<ChatMessageContent>("Streaming", input, cancellationToken))
            if (!string.IsNullOrEmpty(message.Content))
                yield return message;
    }

    private static IEnumerable<string> ConsoleInput(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write("User > ");
            var input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input)) yield return input;
        }
    }
}