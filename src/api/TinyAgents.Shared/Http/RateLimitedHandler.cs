using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace TinyAgents.Shared.Http;

internal sealed class RateLimitedHandler(RateLimiter rateLimiter, ILogger<RateLimitedHandler> logger)
    : DelegatingHandler
{
    private readonly ILogger _logger = logger;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var lease = await rateLimiter.AcquireAsync(1, cancellationToken);
        if (lease.IsAcquired)
        {
            _logger.LogInformation("Rate limit lease acquired");
            return await base.SendAsync(request, cancellationToken);
        }
        
        _logger.LogInformation("Unable to acquire rate limit");
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.TooManyRequests);
        if (lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            response.Headers.Add(
                HeaderNames.RetryAfter, 
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
        }
        
        return response;
    }
}