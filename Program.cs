using AadRagStripeSite.Components;
using AadRagStripeSite.Services;
using AadRagStripeSite.Infrastructure.Options;
using AadRagStripeSite.Services.Stub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;

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
        builder.Services.AddSingleton<IRagChatService, StubRagChatService>();
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.LoginPath = "/auth/signin";
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Authority = builder.Configuration["Authentication:Authority"];
            options.ClientId = builder.Configuration["Authentication:ClientId"];
            options.ClientSecret = builder.Configuration["Authentication:ClientSecret"];
            options.ResponseType = "code";
            options.UsePkce = true;
            options.SaveTokens = true;
            options.CallbackPath = builder.Configuration["Authentication:CallbackPath"] ?? "/signin-oidc";
            options.SignedOutCallbackPath = builder.Configuration["Authentication:SignedOutCallbackPath"] ?? "/signout-callback-oidc";
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
        });

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
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
            return Results.Challenge(new AuthenticationProperties { RedirectUri = redirectUri },
                [OpenIdConnectDefaults.AuthenticationScheme]);
        }).AllowAnonymous();

        app.MapGet("/auth/signout", () =>
        {
            return Results.SignOut(new AuthenticationProperties { RedirectUri = "/" },
                [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
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

            await foreach (var chunk in chatService.StreamAnswerAsync(request, cancellationToken))
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
