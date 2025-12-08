param location string = resourceGroup().location
param baseName string = toLower(replace('${resourceGroup().name}-rag', '_', '-'))

@description('App Service SKU (Linux).')
param appServiceSku string = 'P1v3'

@description('App Service runtime stack.')
param appServiceStack string = 'DOTNETCORE|10.0'

@description('Azure SQL admin login (DO NOT use production secrets here).')
param sqlAdminLogin string = 'sqladminuser'

@secure()
@description('Azure SQL admin password.')
param sqlAdminPassword string

@description('Azure OpenAI SKU.')
@allowed([
  'S0'
])
param openAiSku string = 'S0'

@description('Azure AI Search SKU.')
@allowed([
  'basic'
  'standard'
  'standard2'
  'standard3'
])
param searchSku string = 'basic'

var sanitizedBase = toLower(replace(baseName, '[^a-z0-9-]', ''))
var storageName = toLower(replace('${baseName}storage', '-', ''))
var sqlServerName = toLower(replace('${baseName}-sql', '_', '-'))
var searchName = toLower('${baseName}-search')
var openAiName = toLower('${baseName}-aoai')
var appInsightsName = '${sanitizedBase}-appi'
var keyVaultName = toLower(replace('${baseName}-kv', '_', '-'))
var planName = '${sanitizedBase}-plan'
var webAppName = '${sanitizedBase}-app'

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
          name: 'Azure__Search__Endpoint'
          value: 'https://${searchName}.search.windows.net'
        }
        {
          name: 'Azure__OpenAI__Endpoint'
          value: 'https://${openAiName}.openai.azure.com'
        }
        {
          name: 'ConnectionStrings__Sql'
          value: 'Server=tcp:${sqlServer.name}.database.windows.net,1433;Database=${sqlDatabase.name};Authentication=Active Directory Default;'
        }
      ]
      linuxFxVersion: appServiceStack
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

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    publicNetworkAccess: 'Enabled'
    minimalTlsVersion: '1.2'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  name: '${sqlServer.name}/${baseName}-db'
  location: location
  sku: {
    name: 'GP_Gen5_2'
    tier: 'GeneralPurpose'
  }
  properties: {
    zoneRedundant: false
  }
  dependsOn: [
    sqlServer
  ]
}

resource search 'Microsoft.Search/searchServices@2023-11-01' = {
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

resource openAi 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
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

resource webIdentitySql 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(webIdentity.id, sqlServer.id, 'Azure AD Admin')
  scope: sqlServer
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4d60cc11-67c2-4a5e-9537-6d7e0c4e6d08'
    )
    principalId: webIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource webUpdate 'Microsoft.Web/sites@2023-12-01' existing = {
  name: web.name
}

resource identityAssign 'Microsoft.Web/sites/identity@2023-12-01' = {
  parent: webUpdate
  name: 'default'
  properties: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${webIdentity.id}': {}
    }
  }
}

output webAppUrl string = 'https://${web.properties.defaultHostName}'
output storageAccountName string = storage.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output keyVaultName string = keyVault.name
output searchEndpoint string = 'https://${searchName}.search.windows.net'
output openAiEndpoint string = 'https://${openAiName}.openai.azure.com'
