using System.Net;
using Microsoft.Azure.Cosmos;

namespace AadRagStripeSite.Services.Data;

public sealed class CosmosRepository<T> : ICosmosRepository<T> where T : class
{
    private readonly Container _container;

    public CosmosRepository(Container container)
    {
        _container = container;
    }

    public async Task<T> UpsertAsync(T entity, string partitionKey, CancellationToken cancellationToken)
    {
        var response = await _container.UpsertItemAsync(entity, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
        return response.Resource;
    }

    public async Task<T?> GetAsync(string id, string partitionKey, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
