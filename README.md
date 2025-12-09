# Agent TODO
- Convert chatClient.js to a Blazor-based streaming client
- Implement CI/CD (GitHub Actions) for build/deploy and Bicep validation
- Finish RAG: wire Azure Search + OpenAI clients into RagChatService (replace placeholder chunks)
- Add sign-out UI when user is logged in

# Human TODO
- Upload/index the `Data/Docs` content into Search (or through Storage + indexer)
- (If not using MI) Provide Cosmos key/connection or keep MI role assignment current
- Provide Entra ID / B2C Authority/ClientId/ClientSecret (Key Vault/App Service settings)
- Purge/restore any soft-deleted Azure OpenAI resource or set `createOpenAi=false` and reference an existing one

# Deploy
az deployment group create \
    --resource-group aad-rag-stripe \
    --template-file Bicep/main.bicep \
    --parameters useExistingSearch=true existingSearchEndpoint=https://[myname].search.windows.net createOpenAi=false
