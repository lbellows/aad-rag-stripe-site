using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AadRagStripeSite.Infrastructure.Options;

public static class OptionsExtensions
{
    public static IServiceCollection AddValidatedOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AzureOpenAiOptions>()
            .Bind(configuration.GetSection("Azure:OpenAI"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AzureSearchOptions>()
            .Bind(configuration.GetSection("Azure:Search"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SearchKeyOptions>()
            .Bind(configuration.GetSection("Azure:Search"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AzureStorageOptions>()
            .Bind(configuration.GetSection("Azure:Storage"))
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
            .Configure<IConfiguration>((opts, config) =>
            {
                var section = config.GetSection("Foundry");
                if (!section.Exists())
                {
                    section = config.GetSection("Azure:Foundry");
                }
                section.Bind(opts);
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
