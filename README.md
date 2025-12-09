# Agent TODO
- Review our TODOs and determine what was been implemented.
- Github actions CICD
- Convert chatClient.js to Blazor since it should be able to provide similar functionality
- Use new AI Foundry endpoints (not chat completions?)
- Wire Entra ID / B2C OpenID Connect auth end-to-end (replace stub config values, test signin/signout)
- Swap backend persistence to Cosmos DB SDK and update data services accordingly
- Install missing Nuget packages or update the Human TODO with the details
- We will definitely need a sign out button if the user is logged in.
- Index and test RAG content using provided pilot ops docs in `Data/Docs/*`

# Human TODO
- Provide Entra ID / B2C Authority, ClientId, and ClientSecret values (and configure redirect URLs) for OpenID Connect in `appsettings.*` or Key Vault
- Provide Cosmos primary key/connection string (if using key auth) or assign managed identity permissions to Cosmos and set `Cosmos` config accordingly
- Purge/restore any soft-deleted Azure OpenAI resource or set `createOpenAi=false` and reference an existing one
- Upload documents in `Data/Docs` to your ingestion pipeline (Storage/Search). Example: `az storage blob upload` to a container, then index with Search (admin key) or your ETL.

# Deploy
az deployment group create \
    --resource-group aad-rag-stripe \
    --template-file Bicep/main.bicep \
    --parameters useExistingSearch=true existingSearchEndpoint=https://[myname].search.windows.net createOpenAi=false
