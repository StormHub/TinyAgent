using System.ComponentModel.DataAnnotations;
using Azure.Core.Serialization;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Spatial;

namespace TinyAgents.Search.Azure;

public sealed class LocationIndex
{
    private const string DefaultVectorSearchProfile = "KMDefaultProfile";
    private const string DefaultVectorSearchConfiguration = "KMDefaultAlgorithm";

    // Fixed vector size of ada-002 text embedding model
    private const int DefaultVectorDimensions = 1536;

    [SimpleField(IsKey = true)] public required string Id { get; init; }

    [SearchableField] public required string Name { get; init; }

    [SearchableField] public required string Address { get; init; }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    [DataType("Edm.GeographyPoint")]
    public required GeographyPoint Point { get; init; }

    [SearchableField] public string? Description { get; init; }

    [VectorSearchField(IsHidden = true, VectorSearchDimensions = DefaultVectorDimensions,
        VectorSearchProfileName = DefaultVectorSearchProfile)]
    public float[]? Embedding { get; set; }

    public static SearchIndex Index(string name, JsonObjectSerializer jsonObjectSerializer)
    {
        var builder = new FieldBuilder { Serializer = jsonObjectSerializer };
        var fields = builder.Build(typeof(LocationIndex));

        var index = new SearchIndex(name, fields)
        {
            VectorSearch = new VectorSearch
            {
                Profiles =
                {
                    new VectorSearchProfile(DefaultVectorSearchProfile, DefaultVectorSearchConfiguration)
                },
                Algorithms =
                {
                    new HnswAlgorithmConfiguration(DefaultVectorSearchConfiguration)
                    {
                        Parameters = new HnswParameters
                        {
                            Metric = VectorSearchAlgorithmMetric.Cosine
                        }
                    }
                }
            }
        };

        return index;
    }
}