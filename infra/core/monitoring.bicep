@description('Name of the Application Insights component')
param name string

@description('Location for the Application Insights component')
param location string = resourceGroup().location

@description('Tags for the resources')
param tags object = {}

@description('Application type for Application Insights')
param kind string = 'web'

@description('Log Analytics Workspace name')
param logAnalyticsName string = '${name}-workspace'

// Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
  }
}

// Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  tags: tags
  kind: kind
  properties: {
    Application_Type: kind
    WorkspaceResourceId: logAnalyticsWorkspace.id
    IngestionMode: 'LogAnalytics'
    RetentionInDays: 30
  }
}

output applicationInsightsName string = applicationInsights.name
output applicationInsightsId string = applicationInsights.id
output applicationInsightsConnectionString string = applicationInsights.properties.ConnectionString
output applicationInsightsInstrumentationKey string = applicationInsights.properties.InstrumentationKey
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.properties.customerId
output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name