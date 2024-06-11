using Microsoft.Extensions.Logging;

namespace TinyAgents.Shared.Http;

public sealed class TraceHttpHandler(ILogger<TraceHttpHandler> logger) : DelegatingHandler
{
    private readonly ILogger _logger = logger;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
#if DEBUG
        if (request.Content is not null)
        {
            await using var requestStream = await GetContentStream(request.Content);
            var requestStreamContent = new StreamContent(requestStream);
            var requestContent = await requestStreamContent.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("{RequestUri} {RequestContent}", request.RequestUri, requestContent);
        }
#endif

        var response = await base.SendAsync(request, cancellationToken);

#if DEBUG
        var responseStream = await GetContentStream(response.Content);
        var responseStreamContent = new StreamContent(responseStream);
        var responseContent = await responseStreamContent.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation("{ResponseContent}", responseContent);

        // Rewind the stream position for downstream reads
        responseStream.Position = 0;
        response.Content = responseStreamContent;
#endif

        return response;
    }

    private static async Task<Stream> GetContentStream(HttpContent httpContent)
    {
        var stream = new MemoryStream();
        await httpContent.CopyToAsync(stream);
        stream.Position = 0;
        return stream;
    }
}