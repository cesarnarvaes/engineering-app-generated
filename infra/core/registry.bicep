@description('Name of the Container Registry')
param name string

@description('Location for the Container Registry')
param location string = resourceGroup().location

@description('Tags for the resources')
param tags object = {}

@description('SKU for the Container Registry')
param sku string = 'Basic'

@description('Enable admin user')
param adminUserEnabled bool = true

// Container Registry
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
  }
  properties: {
    adminUserEnabled: adminUserEnabled
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
    zoneRedundancy: 'Disabled'
  }
}

output name string = containerRegistry.name
output loginServer string = containerRegistry.properties.loginServer
output id string = containerRegistry.id