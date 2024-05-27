using Azure;
using Azure.Core.Pipeline;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TinyAgents.Search.Azure;

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
        
        return services;
    }
}