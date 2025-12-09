using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace AadRagStripeSite.Infrastructure.Cosmos;

public static class CosmosClientFactory
{
    public static CosmosClient Create(IOptions<Options.CosmosOptions> optionsAccessor)
    {
        var options = optionsAccessor.Value;

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return new CosmosClient(options.ConnectionString);
        }

        if (!string.IsNullOrWhiteSpace(options.Key))
        {
            return new CosmosClient(options.AccountEndpoint, options.Key);
        }

        throw new InvalidOperationException("Cosmos configuration missing connection information. Provide ConnectionString, Key, or enable managed identity with RBAC.");
    }
}
