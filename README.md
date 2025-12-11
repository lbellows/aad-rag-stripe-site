
# Deploy
az deployment group create \
    --resource-group aad-rag-stripe \
    --template-file Bicep/main.bicep \
    --parameters useExistingSearch=true existingSearchEndpoint=https://[myname].search.windows.net createOpenAi=false
