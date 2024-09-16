using Microsoft.Extensions.DependencyInjection;

namespace TinyAgents.Shared.Http;

public static class DependencyInjection
{
    public static  IHttpClientBuilder AddRateLimitedHttpClient(this IServiceCollection services, string name)
    {
        var builder = services.AddHttpClient(name);
        builder.AddStandardResilienceHandler();
        
#if DEBUG
        services.AddTransient<TraceHttpHandler>();
        builder.AddHttpMessageHandler<TraceHttpHandler>();
#endif
        return builder;
    }
}