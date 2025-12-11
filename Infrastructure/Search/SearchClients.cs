using Microsoft.Extensions.Options;
using AadRagStripeSite.Infrastructure.Options;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;

namespace AadRagStripeSite.Infrastructure.Search;

public static class SearchClients
{
    public static SearchClient CreateSearchClient(IOptions<AzureSearchOptions> endpointOptions, IOptions<SearchKeyOptions> keyOptions)
    {
        var opts = endpointOptions.Value;
        var keys = keyOptions.Value;
        return new SearchClient(new Uri(opts.Endpoint), opts.IndexName, new AzureKeyCredential(keys.QueryKey));
    }
}
