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
            .Validate(opts => ValidateFoundryOptions(opts), "Foundry configuration is incomplete for the selected client.")
            .ValidateOnStart();

        return services;
    }

    private static bool ValidateFoundryOptions(FoundryOptions opts)
    {
        if (opts.UseProjects)
        {
            return !string.IsNullOrWhiteSpace(opts.ProjectEndpoint)
                   && Uri.IsWellFormedUriString(opts.ProjectEndpoint, UriKind.Absolute)
                   && !string.IsNullOrWhiteSpace(opts.AgentName);
        }

        return !string.IsNullOrWhiteSpace(opts.ResponsesEndpoint)
               && Uri.IsWellFormedUriString(opts.ResponsesEndpoint, UriKind.Absolute)
               && !string.IsNullOrWhiteSpace(opts.Scope)
               && Uri.IsWellFormedUriString(opts.Scope, UriKind.Absolute);
    }
}
