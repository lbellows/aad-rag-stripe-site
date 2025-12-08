param location string = resourceGroup().location
param baseName string = toLower(replace('${resourceGroup().name}-rag', '_', '-'))

@description('App Service SKU (Linux). Use B1 for low-cost dev/hobby.')
param appServiceSku string = 'B1'

@description('App Service runtime stack.')
param appServiceStack string = 'DOTNETCORE|10.0'

@description('Azure OpenAI SKU.')
@allowed([
  'S0'
])
param openAiSku string = 'S0'

@description('Azure AI Search SKU.')
@allowed([
  'free'
  'basic'
  'standard'
  'standard2'
  'standard3'
])
param searchSku string = 'free'

@description('Use an existing Azure AI Search endpoint instead of creating one (set to true to skip provisioning).')
param useExistingSearch bool = false

@description('Existing Azure AI Search endpoint (e.g., https://yoursearch.search.windows.net). Used when useExistingSearch = true.')
param existingSearchEndpoint string = ''

@description('Create an Azure OpenAI account. Set false to skip and use an existing account.')
param createOpenAi bool = true

var sanitizedBase = toLower(replace(baseName, '[^a-z0-9-]', ''))
var storageName = toLower(replace('${baseName}storage', '-', ''))
var searchName = toLower('${baseName}-search')
var openAiName = toLower('${baseName}-aoai')
var appInsightsName = '${sanitizedBase}-appi'
var keyVaultName = toLower(replace('${baseName}-kv', '_', '-'))
var planName = '${sanitizedBase}-plan'
var webAppName = '${sanitizedBase}-app'
var cosmosName = toLower(replace('${baseName}-cosmos', '_', '-'))
var cosmosDbName = 'appdb'
var cosmosContainerName = 'items'
var searchEndpoint = useExistingSearch && !empty(trim(existingSearchEndpoint)) ? existingSearchEndpoint : 'https://${searchName}.search.windows.net'
var openAiEndpoint = 'https://${openAiName}.openai.azure.com'

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: planName
  location: location
  sku: {
    name: appServiceSku
    tier: split(appServiceSku, 'v')[0]
  }
  kind: 'linux'
  properties: {
    reserved: true
    perSiteScaling: false
  }
}

resource web 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '0'
        }
        {
          name: 'DOTNET_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'Azure__Storage__AccountName'
          value: storage.name
        }
        {
          name: 'Cosmos__AccountEndpoint'
          value: cosmos.properties.documentEndpoint
        }
        {
          name: 'Cosmos__Database'
          value: cosmosDbName
        }
        {
          name: 'Cosmos__Container'
          value: cosmosContainerName
        }
        {
          name: 'Azure__Search__Endpoint'
          value: searchEndpoint
        }
        {
          name: 'Azure__OpenAI__Endpoint'
          value: openAiEndpoint
        }
      ]
      linuxFxVersion: appServiceStack
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${webIdentity.id}': {}
    }
  }
}

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    enableRbacAuthorization: true
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enablePurgeProtection: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    IngestionMode: 'ApplicationInsights'
  }
}

resource search 'Microsoft.Search/searchServices@2023-11-01' = if (!useExistingSearch) {
  name: searchName
  location: location
  sku: {
    name: searchSku
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    disableLocalAuth: true
    publicNetworkAccess: 'enabled'
  }
}

resource openAi 'Microsoft.CognitiveServices/accounts@2023-05-01' = if (createOpenAi) {
  name: openAiName
  location: location
  kind: 'OpenAI'
  sku: {
    name: openAiSku
  }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

resource cosmos 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    databaseAccountOfferType: 'Standard'
    enableFreeTier: true
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    publicNetworkAccess: 'Enabled'
    disableKeyBasedMetadataWriteAccess: false
    enableAutomaticFailover: false
  }
}

resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  parent: cosmos
  name: cosmosDbName
  properties: {
    resource: {
      id: cosmosDbName
    }
  }
}

resource cosmosContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: cosmosDb
  name: cosmosContainerName
  properties: {
    resource: {
      id: cosmosContainerName
      partitionKey: {
        paths: [
          '/pk'
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
      }
    }
    options: {}
  }
}

resource webIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${webAppName}-id'
  location: location
}

resource webIdentityRole 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(webIdentity.id, storage.id, 'Storage Blob Data Reader')
  scope: storage
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
    )
    principalId: webIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource webIdentityKeyVault 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(webIdentity.id, keyVault.id, 'Key Vault Secrets User')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6'
    )
    principalId: webIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

output webAppUrl string = 'https://${web.properties.defaultHostName}'
output storageAccountName string = storage.name
output keyVaultName string = keyVault.name
output searchEndpointOutput string = searchEndpoint
output openAiEndpointOutput string = openAiEndpoint
output cosmosEndpoint string = cosmos.properties.documentEndpoint
