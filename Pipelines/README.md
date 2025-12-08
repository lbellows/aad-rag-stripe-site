# Pipelines

Use GitHub Actions to build and deploy:

- Restore/build the `AadRagStripeSite.csproj` targeting `net10.0`.
- Publish artifacts for Linux (App Service).
- Deploy Bicep templates with the Azure CLI login action.

Keep pipeline secrets (Azure credentials, Stripe keys) in GitHub encrypted secrets or Azure Key Vault; never commit them here.
