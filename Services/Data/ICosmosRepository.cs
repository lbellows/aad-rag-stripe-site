namespace AadRagStripeSite.Services.Data;

public interface ICosmosRepository<T>
{
    Task<T> UpsertAsync(T entity, string partitionKey, CancellationToken cancellationToken);
    Task<T?> GetAsync(string id, string partitionKey, CancellationToken cancellationToken);
}
