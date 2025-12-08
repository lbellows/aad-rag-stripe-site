using AadRagStripeSite.Components;
using AadRagStripeSite.Services;
using AadRagStripeSite.Services.Stub;
using AadRagStripeSite.Infrastructure.Options;

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

        app.UseAntiforgery();

        app.MapGet("/healthz", () => Results.Ok(new { status = "ok", environment = app.Environment.EnvironmentName }));

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
