# Infrastructure (Bicep)

This folder will hold Azure Bicep templates for the Blazor web app, including:

- App Service plan and Linux Web App for the Blazor Server site.
- Azure SQL Database for app data and subscription state.
- Storage account for blobs and RAG source documents.
- Azure AI Search + Azure OpenAI for the chatbot pipeline.
- Key Vault for secrets and Stripe webhook signing keys.

Deploy via `az deployment sub create` or `az deployment group create` depending on scope. Keep secrets out of source control; reference Key Vault or deployment parameters stored securely.
