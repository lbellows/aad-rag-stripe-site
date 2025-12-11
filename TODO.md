# Agent TODO
- Convert chatClient.js to a Blazor-based streaming client
- Implement CI/CD (GitHub Actions) for build/deploy and Bicep validation
- Finish RAG: wire Azure Search + OpenAI clients into RagChatService (replace placeholder chunks)
- Add sign-out UI when user is logged in
- Change content to be advertising an advanced chat bot with professional expert airline pilot experience
- scroll chat on new chat
- typing indicator while chat response loading
- markdown rendering

## Wishlist (future)
- Enable streaming from Foundry Responses endpoint (if supported) to SSE UI
- Hook direct Search/OpenAI path later if desired

# Human TODO
- Upload/index the `Data/Docs` content into Search (or through Storage + indexer)
- (If not using MI) Provide Cosmos key/connection or keep MI role assignment current
- Provide Entra ID / B2C Authority/ClientId/ClientSecret (Key Vault/App Service settings)
- Purge/restore any soft-deleted Azure OpenAI resource or set `createOpenAi=false` and reference an existing one
