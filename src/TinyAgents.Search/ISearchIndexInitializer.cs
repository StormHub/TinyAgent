namespace TinyAgents.Search;

public interface ISearchIndexInitializer
{
    Task EnsureExists(CancellationToken cancellationToken = default);
}