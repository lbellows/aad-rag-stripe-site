# Agent TODO
- Create BICEP Templates
- Github actions CICD
- Convert chatClient.js to Blazor since it should be able to provide similar functionality
- Use new AI Foundry endpoints (not chat completions?)
- Wire Entra ID / B2C OpenID Connect auth end-to-end (replace stub config values, test signin/signout)
- Redeploy Bicep after fixes (SQL AAD admin manual step)
- Swap backend persistence to Cosmos DB SDK and update data services accordingly
- Update Bicep parameters for deployment: useExistingSearch=true (resumeai.search.windows.net), createOpenAi=false unless purged/restored
- Install missing Nuget packages or update the Human TODO with the details

# Human TODO
- Execute BICEP templates
- Provide Entra ID / B2C Authority, ClientId, and ClientSecret values (and configure redirect URLs) for OpenID Connect in `appsettings.*` or Key Vault
- Provide Cosmos primary key/connection string (if using key auth) or assign managed identity permissions to Cosmos and set `Cosmos` config accordingly
- Purge/restore any soft-deleted Azure OpenAI resource or set `createOpenAi=false` and reference an existing one
