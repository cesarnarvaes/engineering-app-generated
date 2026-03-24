@description('Name of the Container Apps Environment')
param name string

@description('Location for the Container Apps Environment')
param location string = resourceGroup().location

@description('Tags for the resources')
param tags object = {}

@description('Log Analytics Workspace ID')
param logAnalyticsWorkspaceId string = ''

@description('Zone redundancy setting')
param zoneRedundant bool = false

// Container Apps Environment
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    zoneRedundant: zoneRedundant
    appLogsConfiguration: !empty(logAnalyticsWorkspaceId) ? {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspaceId
      }
    } : {}
  }
}

output id string = containerAppsEnvironment.id
output name string = containerAppsEnvironment.name
output defaultDomain string = containerAppsEnvironment.properties.defaultDomain