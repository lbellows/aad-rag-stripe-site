# Infrastructure (Bicep)

## Files
- `main.bicep` – Resource-group deployment that provisions:
  - Linux App Service Plan + Web App
  - Azure SQL Server + Database
  - Storage Account
  - Azure AI Search
  - Azure OpenAI
  - Key Vault (RBAC)
  - Application Insights
  - User-assigned managed identity for the Web App with storage/KV/SQL role assignments

## Parameters (main.bicep)
- `location` (default: resource group)
- `baseName` (default: `<rg>-rag`)
- `appServiceSku` (default: B1 for low-cost dev/hobby)
- `appServiceStack` (default: DOTNETCORE|10.0)
- `openAiSku` (default: S0)
- `searchSku` (default: free)
- `useExistingSearch` (default: false) and `existingSearchEndpoint` to point at a pre-existing Search service (skip provisioning).
- `createOpenAi` (default: true) to skip creating Azure OpenAI if you already have one (and to avoid soft-delete conflicts).
- Cosmos resources are deployed in serverless + free tier mode. No SQL is provisioned.

## Deploy (resource-group scope)
```bash
RG=<your-resource-group>
az deployment group create \
  --resource-group $RG \
  --template-file Bicep/main.bicep \
  --parameters useExistingSearch=true existingSearchEndpoint=https://resumeai.search.windows.net createOpenAi=false
```

## Post-deploy wiring
- Provide secrets (OpenAI deployment name, Search admin key, Stripe keys, Cosmos key/connection) via Key Vault or App Service settings.
- Create OpenAI deployments (e.g., gpt-4o) after the resource exists.
- Adjust network restrictions as needed (disable public endpoints where appropriate).

## Notes on Quotas/Reuse
- Free Search tier allows only one service per subscription; set `useExistingSearch=true` and pass your existing endpoint to skip provisioning.
- Azure OpenAI may be soft-deleted; set `createOpenAi=false` and reference an existing account (or purge/restore manually).
