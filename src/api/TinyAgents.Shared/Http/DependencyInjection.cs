using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace TinyAgents.Shared.Http;

public static class DependencyInjection
{
    public static  IHttpClientBuilder AddRateLimitedHttpClient(this IServiceCollection services, string name)
    {
        var builder = services.AddHttpClient(name);
        
#if DEBUG
        services.AddTransient<TraceHttpHandler>();
        builder.AddHttpMessageHandler<TraceHttpHandler>();
#endif

        services.AddTransient<RateLimiter>(_ => new TokenBucketRateLimiter(
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = 1,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 1,
                ReplenishmentPeriod = TimeSpan.FromSeconds(3),
                TokensPerPeriod = 1,
                AutoReplenishment = true
            }));
        
        services.AddTransient<RateLimitedHandler>();
        builder.AddHttpMessageHandler<RateLimitedHandler>();

        builder.AddPolicyHandler(
            HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        return builder;
    }
}