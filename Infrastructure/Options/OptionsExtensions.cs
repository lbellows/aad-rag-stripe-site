using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AadRagStripeSite.Infrastructure.Options;

public static class OptionsExtensions
{
    public static IServiceCollection AddValidatedOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AzureOpenAiOptions>()
            .Bind(configuration.GetSection("OpenAI"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AzureSearchOptions>()
            .Bind(configuration.GetSection("Search"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SearchKeyOptions>()
            .Bind(configuration.GetSection("Search"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AzureStorageOptions>()
            .Bind(configuration.GetSection("Storage"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<StripeOptions>()
            .Bind(configuration.GetSection("Stripe"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<UsageLimitOptions>()
            .Bind(configuration.GetSection("UsageLimits"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CosmosOptions>()
            .Bind(configuration.GetSection("Cosmos"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<FoundryOptions>()
            .Bind(configuration.GetSection("Foundry"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
