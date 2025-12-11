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
- **Quota stub**: `InMemorySubscriptionService` enforces per-user quotas (daily reset) using `UsageLimits` from configuration. Replace with DB-backed entitlements driven by Stripe webhooks.
- **Auth skeleton**: Cookie + OpenID Connect (Entra/B2C) configured via `Authentication` settings. Fallback policy is permissive during integration; `/auth/signin` triggers OIDC when configured. Add `[Authorize]` back to `/app` when ready.
- **Data persistence**: Cosmos DB (serverless, free tier). Chat messages are stored via `CosmosChatRepository` (db `appdb`, container `items`).
- **Search/OpenAI quotas**: Free Search is limited to one service per subscription; use `useExistingSearch=true` and `existingSearchEndpoint` to reuse. Azure OpenAI may be soft-deleted; set `createOpenAi=false` and point to an existing account (or purge/restore manually).
- **Foundry agent**: Foundry chat can run via the HTTP Responses endpoint (`FoundryAgentClient`) or via Azure.AI.Projects (`MSFoundryAgentClient`). Toggle with `Foundry:UseProjects=true`. Required config: `ProjectEndpoint` + `AgentName` for the SDK path; or `ResponsesEndpoint` + `Scope` (+ optional `Model`) for HTTP. `/pilot-chat` uses the agent directly; `/api/chat/stream` now uses the agent (single SSE chunk) and persists history in Cosmos. Replace placeholder chunking later with streaming if needed.

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
- `appsettings.json`/`appsettings.Development.json` include placeholders for OpenAI/Search/Storage, Stripe keys, `UsageLimits`, `Authentication`, `Cosmos`, and `Foundry` (toggle `UseProjects`, `ProjectEndpoint`, `AgentName`, or `ResponsesEndpoint`/`Scope`). Bindings use validated options in `Program.cs` (`AddValidatedOptions`). Real values should come from environment variables, Key Vault references, or user secrets.
- Secrets for local/dev are currently in `appsettings.Development.json` (gitignored). Cosmos connection string is present there for local connectivity. Keep production secrets in Key Vault/App Service settings.

## Content for RAG
- Pilot operations docs available in `Data/Docs/` (foundations, dispatch/performance, abnormal/emergencies). Use these for initial indexing in Search/Storage.

## Workflow Notes
- Tasks are tracked in `README.md` under **Agent TODO** (developer-owned) and **Human TODO** (requires secrets/permissions). Add items there when work is blocked by network/permissions or needs human input.
- Bicep deploy notes: SQL AAD admin must be configured manually (portal/CLI) post-deploy. Role assignment for SQL admin was removed; rerun deployment after manual SQL admin config if needed.
- Auth: Currently OIDC is configured only when `Authentication:Authority`/`ClientId` are set. Fallback policy is permissive; `/auth/signin` challenges OIDC when configured. `[Authorize]` on `/app` was removed to keep public during integration; add back once ready. `GetClaimsFromUserInfoEndpoint` is enabled.
- Data: Cosmos serverless (free tier) is provisioned; Cosmos connection string is in gitignored `appsettings.Development.json`. Chat messages use `CosmosChatRepository` (container `items`, DB `appdb` by default). Search/OpenAI configs expect endpoints + keys in config/Key Vault.
- RAG status: `RagChatService` now calls the Foundry agent (Responses endpoint) and persists chat history to Cosmos; SSE endpoint returns a single chunk. Pilot docs live in `Data/Docs/*` for initial indexing. Can swap to direct Search+OpenAI in the future.
- Infra: Using existing free Search (`useExistingSearch` param), and optionally skipping OpenAI creation (`createOpenAi=false`) to avoid soft-delete conflicts. App Service plan default is B1; Cosmos is serverless free tier.
- Secrets: `appsettings.Development.json` (gitignored) holds dev secrets. Production secrets should go in Key Vault/App Service settings. Search keys are expected under `Search:AdminKey`/`Search:QueryKey`; OpenAI can use `ApiKey` if not MI.
- When adding pipelines, ensure steps work on Linux runners, publish for `net10.0`, and deploy artifacts to the Azure Web App.

## Next Steps for Contributors
1. Wire authentication (Entra ID/B2C) and gate `/app`.
2. Scaffold `IRagChatService` + SSE Minimal API for chat streaming.
3. Add EF Core models for users/subscriptions/quotas in `Data/`, plus migrations targeting Azure SQL.
4. Integrate Stripe (SDK) with Checkout + webhooks and map entitlements in `ISubscriptionService`.
5. Expand `Bicep/` templates and `Pipelines/` workflows to deploy the full stack.
