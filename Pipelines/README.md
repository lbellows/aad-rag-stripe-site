# Pipelines

Use GitHub Actions to build, deploy, and validate infra.

## Workflows
- `.github/workflows/build-and-deploy.yml` – Restores/builds/publishes `AadRagStripeSite.csproj` (net10.0) and deploys the zip to Azure App Service.
- `.github/workflows/bicep-validate.yml` – Runs `az deployment group what-if` against `Bicep/main.bicep` for PRs/tags.

## Required GitHub secrets
- `AZURE_CREDENTIALS`: JSON output from `az ad sp create-for-rbac` with access to the subscription/resource group.
- `AZURE_RESOURCE_GROUP`: Target resource group for the web app + Bicep validation.
- `AZURE_WEBAPP_NAME`: App Service name (Linux) for deploy.
- Optional: `AZURE_LOCATION` (if you need to force a location for what-if) and `BICEP_PARAMETERS` (extra `--parameters` for main.bicep).

Keep secrets (Azure credentials, Stripe keys) in GitHub encrypted secrets or Azure Key Vault; never commit them here.
