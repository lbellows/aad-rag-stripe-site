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
- `appServiceSku` (default: P1v3)
- `appServiceStack` (default: DOTNETCORE|8.0)
- `sqlAdminLogin` / `sqlAdminPassword` (secure)
- `openAiSku` (default: S0)
- `searchSku` (default: basic)

## Deploy (resource-group scope)
```bash
RG=<your-resource-group>
az deployment group create \
  --resource-group $RG \
  --template-file Bicep/main.bicep \
  --parameters sqlAdminPassword=<SecurePassword>
```

## Post-deploy wiring
- Provide secrets (OpenAI deployment name, Search admin key, Stripe keys) via Key Vault or App Service settings.
- Configure Azure AD admin for SQL if using AAD auth.
- Create OpenAI deployments (e.g., gpt-4o) after the resource exists.
- Adjust network restrictions as needed (disable public endpoints where appropriate).
