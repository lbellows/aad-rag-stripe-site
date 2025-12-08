# AGENTS GUIDE

This repo hosts a Blazor Web App (Server interactivity) targeting `net10.0` / C# 14 for Azure App Service (Linux). The goal is a RAG-powered chatbot gated by Stripe subscriptions with Azure-native identity.

## Solution Layout
- `AadRagStripeSite.sln` and `AadRagStripeSite.csproj` live at the repo root for Linux-focused development.
- `Components/` – Layout primitives (header, nav, reconnection modal) and shared UI.
- `Pages/` – Page-level components with code-behind partial classes (Home, App shell, auth placeholders, error/not-found).
- `Services/` – Application services (auth, RAG/chat, Stripe billing, subscriptions).
- `Data/` – EF Core context and entities (user profiles, entitlements, chat quotas).
- `Infrastructure/` – Azure integrations (Key Vault, Blob Storage, AI Search, Azure OpenAI, health checks).
- `Bicep/` – IaC for App Service plan, Web App (Linux), SQL DB, Storage, AI Search, Azure OpenAI, Key Vault, and optional background workers.
- `Pipelines/` – GitHub Actions for build/publish/deploy (Linux).
- `wwwroot/` – Static assets and global dark theme CSS.
- `Styles/` – Shared CSS tokens/utilities if additional global styling is needed.

## Architectural Intent
- **Authentication/Registration**: Use Azure Entra ID or Azure AD B2C. Provide routes `/signin` and `/register`; hook up OIDC flows and ensure an app-level user record + Stripe customer ID is created/updated on success.
- **App Area**: Protected content under `/app`. Apply `[Authorize]` once authentication is wired. Use persistent component state for the chat workspace so reconnects or refreshes do not lose context.
- **RAG Chatbot**: Implement `IRagChatService` that queries Azure AI Search (semantic/vector), builds grounded prompts, and calls Azure OpenAI. Expose a Minimal API endpoint (e.g., `/api/chat`) that streams responses via SSE. Enforce per-user quotas (free vs. paid) server-side.
- **Billing**: Implement `IStripeService` to handle Checkout session creation and webhook validation, and `ISubscriptionService` to map Stripe subscription status to entitlements. Store Stripe customer/subscription IDs in the app DB. Webhooks should update subscription state and quotas authoritatively.
- **Current stubs**: `Stub*` services are registered in `Program.cs` to keep the app compiling and provide placeholder responses (auth, chat, subscription checks, Stripe). `/api/chat/stream` emits canned SSE chunks with a dummy quota check—replace once real integrations are added.

## Coding Patterns
- Use Blazor code-behind (`.razor` + `.razor.cs`) to keep markup and logic separate.
- Register services in `Program.cs` via DI; prefer interfaces for all app services (`IAuthService`, `IRagChatService`, `ISubscriptionService`, `IStripeService`, `IUserProfileService`).
- Favor small, reusable components with component-scoped CSS; keep global layout/theme rules in `wwwroot/app.css`.
- Keep secrets out of code. Expect configuration via environment variables, Azure Key Vault references, or user secrets (`dotnet user-secrets`) ignored by git.

## Deployment Notes
- Target Linux App Service; ensure `DOTNET_CLI_HOME` is set when running CLI in restricted environments.
- Add a health endpoint (e.g., `/healthz`) before production deployment. (Implemented as a basic JSON 200 today.)

## UI/Client Notes
- `Components/Chat/RagChat` streams from `/api/chat/stream` using a JS fetch-based SSE helper (`wwwroot/js/chatClient.js`). Replace the stub RAG service with Azure AI Search + Azure OpenAI and preserve the streaming contract.
- `Components/Subscriptions/SubscriptionSummary` uses DI to display current subscription/quota info from `ISubscriptionService`. Replace stub logic with DB + Stripe-driven entitlements.

## Configuration
- `appsettings.json`/`appsettings.Development.json` include placeholders for Azure OpenAI/Search/Storage and Stripe keys. Bindings use validated options in `Program.cs` (`AddValidatedOptions`). Real values should come from environment variables, Key Vault references, or user secrets.
- When adding pipelines, ensure steps work on Linux runners, publish for `net10.0`, and deploy artifacts to the Azure Web App.

## Next Steps for Contributors
1. Wire authentication (Entra ID/B2C) and gate `/app`.
2. Scaffold `IRagChatService` + SSE Minimal API for chat streaming.
3. Add EF Core models for users/subscriptions/quotas in `Data/`, plus migrations targeting Azure SQL.
4. Integrate Stripe (SDK) with Checkout + webhooks and map entitlements in `ISubscriptionService`.
5. Expand `Bicep/` templates and `Pipelines/` workflows to deploy the full stack.
