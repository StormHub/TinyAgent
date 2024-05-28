using Azure;
using Azure.Core.Pipeline;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using TinyAgents.Search.Azure;
using TinyAgents.Search.Resources;

namespace TinyAgents.Search;

public static class DependencyInjection
{
    public static IServiceCollection AddSearch(this IServiceCollection services)
    {
        services.AddOptions<IndexOptions>()
            .BindConfiguration(nameof(IndexOptions))
            .ValidateDataAnnotations();

        services.AddHttpClient(nameof(SearchIndexClient));

        services.AddTransient<SearchIndexClient>(provider =>
        {
            var indexOptions = provider.GetRequiredService<IOptions<IndexOptions>>().Value;

            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var searchOptions = new SearchClientOptions
            {
                Serializer = IndexOptions.JsonObjectSerializer,
                Transport = new HttpClientTransport(factory.CreateClient(nameof(SearchIndexClient)))
            };

            return new SearchIndexClient(
                indexOptions.Uri,
                new AzureKeyCredential(indexOptions.ApiKey),
                searchOptions);
        });

        services.AddTransient<SearchPlugin>();
        services.AddTransient<IndexBuilder>();

        return services;
    }

    public static IKernelBuilder ConfigureLocationPlugin(this IKernelBuilder builder, IServiceProvider provider)
    {
        var searchPlugin = provider.GetRequiredService<SearchPlugin>();
        builder.Plugins.AddFromObject(searchPlugin);
        return builder;
    }

    public static async Task EnsureIndexExists(this IServiceProvider provider, string textEmbeddingModelId)
    {
        var indexBuilder = provider.GetRequiredService<IndexBuilder>();
        await indexBuilder.EnsureExists(textEmbeddingModelId);
    }
}