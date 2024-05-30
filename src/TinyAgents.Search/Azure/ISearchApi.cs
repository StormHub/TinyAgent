namespace TinyAgents.Search.Azure;

public interface ISearchApi
{
    Task<GetLocationsResponse> GetLocations(GetLocationsRequest request, CancellationToken cancellationToken = default);
}