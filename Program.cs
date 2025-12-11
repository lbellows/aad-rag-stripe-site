using AadRagStripeSite.Components;
using AadRagStripeSite.Services;
using AadRagStripeSite.Infrastructure.Options;
using AadRagStripeSite.Services.Stub;
using AadRagStripeSite.Infrastructure.Cosmos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace AadRagStripeSite;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddValidatedOptions(builder.Configuration);
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<IAuthService, StubAuthService>();
        builder.Services.AddSingleton<IUserProfileService, StubUserProfileService>();
        builder.Services.AddSingleton<ISubscriptionService, InMemorySubscriptionService>();
        builder.Services.AddSingleton<IStripeService, StubStripeService>();
        builder.Services.AddSingleton<IRagChatService, RagChatService>();
        builder.Services.AddSingleton(sp => CosmosClientFactory.Create(sp.GetRequiredService<IOptions<CosmosOptions>>()));
        builder.Services.AddSingleton(sp =>
        {
            var cosmosOptions = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;
            var client = sp.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var container = client.GetContainer(cosmosOptions.Database, cosmosOptions.Container);
            return container;
        });
        builder.Services.AddSingleton<AadRagStripeSite.Services.Data.IChatRepository, AadRagStripeSite.Services.Data.CosmosChatRepository>();
        builder.Services.AddScoped<ChatSessionService>();
        builder.Services.AddHttpClient<AadRagStripeSite.Services.Foundry.FoundryAgentClient>();
        builder.Services.AddSingleton<AadRagStripeSite.Services.Foundry.IFoundryAgentClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<AadRagStripeSite.Infrastructure.Options.FoundryOptions>>().Value;
            if (opts.UseProjects)
            {
                return ActivatorUtilities.CreateInstance<AadRagStripeSite.Services.Foundry.MSFoundryAgentClient>(sp);
            }

            return sp.GetRequiredService<AadRagStripeSite.Services.Foundry.FoundryAgentClient>();
        });
        builder.Services.AddCascadingAuthenticationState();

        var authSection = builder.Configuration.GetSection("Authentication");
        var authority = authSection["Authority"];
        var clientId = authSection["ClientId"];
        var clientSecret = authSection["ClientSecret"];
        var oidcConfigured = !string.IsNullOrWhiteSpace(authority) && !string.IsNullOrWhiteSpace(clientId);

        var authBuilder = builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = oidcConfigured ? OpenIdConnectDefaults.AuthenticationScheme : CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.LoginPath = "/auth/signin";
        });

        if (oidcConfigured)
        {
            authBuilder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = authority;
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.ResponseType = "code";
                options.UsePkce = true;
                options.SaveTokens = true;
                options.CallbackPath = authSection["CallbackPath"] ?? "/signin-oidc";
                options.SignedOutCallbackPath = authSection["SignedOutCallbackPath"] ?? "/signout-callback-oidc";
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.GetClaimsFromUserInfoEndpoint = true;
            });
        }

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapGet("/healthz", () => Results.Ok(new { status = "ok", environment = app.Environment.EnvironmentName }))
            .AllowAnonymous();

        app.MapGet("/auth/signin", (string? returnUrl) =>
        {
            var redirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/app" : returnUrl;
            if (oidcConfigured)
            {
                return Results.Challenge(new AuthenticationProperties { RedirectUri = redirectUri },
                    [OpenIdConnectDefaults.AuthenticationScheme]);
            }

            // Avoid redirect loops when auth isn't configured locally.
            return Results.Redirect("/");
        }).AllowAnonymous();

        app.MapGet("/auth/signout", () =>
        {
            return oidcConfigured
                ? Results.SignOut(new AuthenticationProperties { RedirectUri = "/" },
                    [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme])
                : Results.SignOut(new AuthenticationProperties { RedirectUri = "/" },
                    [CookieAuthenticationDefaults.AuthenticationScheme]);
        }).AllowAnonymous();

        app.MapPost("/api/chat/stream", async (Services.Models.ChatRequest request, HttpContext context, IRagChatService chatService, IAuthService authService, ISubscriptionService subscriptionService, CancellationToken cancellationToken) =>
        {
            var user = await authService.GetUserAsync(context.User, cancellationToken);
            var subscription = await subscriptionService.GetSubscriptionAsync(user, cancellationToken);

            if (!subscription.IsActive || subscription.RemainingMessages <= 0)
            {
                return Results.StatusCode(StatusCodes.Status429TooManyRequests);
            }

            context.Response.Headers.CacheControl = "no-cache";
            context.Response.Headers.Connection = "keep-alive";
            context.Response.ContentType = "text/event-stream";

            await foreach (var chunk in chatService.StreamAnswerAsync(request, user.UserId ?? "anon", cancellationToken))
            {
                await context.Response.WriteAsync($"data: {chunk}\n\n", cancellationToken);
                await context.Response.Body.FlushAsync(cancellationToken);
            }

            await subscriptionService.TryConsumeMessageAsync(user, cancellationToken);
            return Results.Empty;
        });

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
