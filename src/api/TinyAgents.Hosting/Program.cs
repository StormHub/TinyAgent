﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel.ChatCompletion;
using TinyAgents.Hosting;
using TinyAgents.SemanticKernel;
using TinyAgents.SemanticKernel.Agents;

IHost? host = default;

try
{
    host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((builderContext, builder) =>
        {
            builder.AddJsonFile("appsettings.json", false);
            builder.AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json", true);

            if (builderContext.HostingEnvironment.IsDevelopment()) builder.AddUserSecrets<Program>();

            builder.AddEnvironmentVariables();
        })
        .ConfigureServices((builderContext, services) =>
        {
            services.AddAgents(builderContext.HostingEnvironment);
        }).Build();

    await host.StartAsync();

    var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
    await using (var scope = host.Services.CreateAsyncScope())
    {
        var history = new ChatHistory();
        var factory = scope.ServiceProvider.GetRequiredService<SearchAgentFactory>();

        var agentProxy = await factory.CreateAgent(history);
        history.AddUserMessage(
            $"""
            Question:
            What are the charger types of tesla model 3?

            Respond in JSON format with the following JSON schema:
            {JsonResponse.JsonSchema()}
            """);

        var response = agentProxy.Invoke(cancellationToken: lifetime.ApplicationStopping);
        await foreach (var message in response)
        {
            Console.WriteLine(message.Content);
        }
    }

    lifetime.StopApplication();
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