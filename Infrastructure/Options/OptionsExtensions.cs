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

        services.AddOptions<AzureStorageOptions>()
            .Bind(configuration.GetSection("Azure:Storage"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<StripeOptions>()
            .Bind(configuration.GetSection("Stripe"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
