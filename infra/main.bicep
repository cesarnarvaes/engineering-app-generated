targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment for resources')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Application name used for resource naming')
param applicationName string = 'bizcrm'

@description('SKU for the SQL Database')
param sqlDatabaseSku string = 'S2'

@description('Administrator login for SQL Server')
param sqlAdminLogin string = 'sqladmin'

@secure()
@description('Administrator password for SQL Server')
param sqlAdminPassword string

@secure()
@description('JWT secret key for authentication')
param jwtSecretKey string

@description('Enable Application Insights monitoring')
param enableMonitoring bool = true

@description('Enable Key Vault for secrets management')
param enableKeyVault bool = true

// Variables
var abbrs = loadJsonContent('abbreviations.json')
var resourceToken = toLower(uniqueString(environmentName, location))
var tags = {
  'azd-env-name': environmentName
  'app-name': applicationName
}

// Resource Group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${abbrs.resourcesResourceGroups}${applicationName}-${environmentName}'
  location: location
  tags: tags
}

// Container Registry
module containerRegistry 'core/registry.bicep' = {
  name: 'container-registry'
  scope: rg
  params: {
    name: '${abbrs.containerRegistryRegistries}${resourceToken}'
    location: location
    tags: tags
  }
}

// Key Vault (optional)
module keyVault 'core/keyvault.bicep' = if (enableKeyVault) {
  name: 'key-vault'
  scope: rg
  params: {
    name: '${abbrs.keyVaultVaults}${resourceToken}'
    location: location
    tags: tags
  }
}

// Application Insights (optional)
module monitoring 'core/monitoring.bicep' = if (enableMonitoring) {
  name: 'monitoring'
  scope: rg
  params: {
    name: '${abbrs.insightsComponents}${resourceToken}'
    location: location
    tags: tags
  }
}

// SQL Server and Database
module database 'core/database.bicep' = {
  name: 'database'
  scope: rg
  params: {
    serverName: '${abbrs.sqlServers}${resourceToken}'
    databaseName: '${abbrs.sqlServersDatabases}${applicationName}'
    location: location
    tags: tags
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    databaseSku: sqlDatabaseSku
  }
}

// Container Apps Environment
module containerAppsEnvironment 'core/container-apps-environment.bicep' = {
  name: 'container-apps-environment'
  scope: rg
  params: {
    name: '${abbrs.appManagedEnvironments}${resourceToken}'
    location: location
    tags: tags
    logAnalyticsWorkspaceId: enableMonitoring ? monitoring.outputs.logAnalyticsWorkspaceId : ''
  }
}

// Backend API (Container App)
module backendApi 'app/backend.bicep' = {
  name: 'backend-api'
  scope: rg
  params: {
    name: '${abbrs.appContainerApps}backend-${resourceToken}'
    location: location
    tags: tags
    containerAppsEnvironmentId: containerAppsEnvironment.outputs.id
    containerRegistryName: containerRegistry.outputs.name
    applicationInsightsConnectionString: enableMonitoring ? monitoring.outputs.applicationInsightsConnectionString : ''
    keyVaultName: enableKeyVault ? keyVault.outputs.name : ''
    sqlConnectionString: database.outputs.connectionString
    jwtSecretKey: jwtSecretKey
    exists: false
  }
}

// Frontend (Static Web App)
module frontend 'app/frontend.bicep' = {
  name: 'frontend'
  scope: rg
  params: {
    name: '${abbrs.webStaticSites}${resourceToken}'
    location: location
    tags: tags
    backendApiUrl: backendApi.outputs.uri
  }
}

// Output values
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_RESOURCE_GROUP string = rg.name

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.outputs.loginServer
output AZURE_CONTAINER_REGISTRY_NAME string = containerRegistry.outputs.name

output AZURE_SQL_SERVER_NAME string = database.outputs.serverName
output AZURE_SQL_DATABASE_NAME string = database.outputs.databaseName

output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = containerAppsEnvironment.outputs.id
output AZURE_BACKEND_API_NAME string = backendApi.outputs.name
output AZURE_BACKEND_API_URL string = backendApi.outputs.uri

output AZURE_FRONTEND_NAME string = frontend.outputs.name
output AZURE_FRONTEND_URL string = frontend.outputs.uri

output AZURE_KEY_VAULT_NAME string = enableKeyVault ? keyVault.outputs.name : ''
output AZURE_APPLICATION_INSIGHTS_NAME string = enableMonitoring ? monitoring.outputs.applicationInsightsName : ''
output AZURE_LOG_ANALYTICS_WORKSPACE_ID string = enableMonitoring ? monitoring.outputs.logAnalyticsWorkspaceId : ''