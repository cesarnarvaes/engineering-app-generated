@description('Name of the Static Web App')
param name string

@description('Location for the Static Web App')
param location string = resourceGroup().location

@description('Tags for the resources')
param tags object = {}

@description('SKU for the Static Web App')
param sku string = 'Standard'

@description('Backend API URL')
param backendApiUrl string

@description('Repository URL (optional)')
param repositoryUrl string = ''

@description('Repository branch (optional)')
param repositoryBranch string = 'main'

// Static Web App
resource staticWebApp 'Microsoft.Web/staticSites@2023-01-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
    tier: sku
  }
  properties: {
    buildProperties: {
      appLocation: '/src/frontend'
      apiLocation: ''
      outputLocation: 'dist'
      appBuildCommand: 'npm run build'
      apiBuildCommand: ''
      skipGithubActionWorkflowGeneration: true
    }
    repositoryUrl: repositoryUrl
    branch: repositoryBranch
    provider: 'DevOps'
    enterpriseGradeCdnStatus: 'Enabled'
    allowConfigFileUpdates: true
    stagingEnvironmentPolicy: 'Enabled'
  }
}

// App Settings for Static Web App
resource appSettings 'Microsoft.Web/staticSites/config@2023-01-01' = {
  parent: staticWebApp
  name: 'appsettings'
  properties: {
    VITE_API_BASE_URL: backendApiUrl
    VITE_APP_NAME: 'BizCrm'
    VITE_APP_VERSION: '1.0.0'
    VITE_ENVIRONMENT: 'production'
  }
}

// Function app settings for backend proxy (if needed)
resource functionAppSettings 'Microsoft.Web/staticSites/config@2023-01-01' = {
  parent: staticWebApp
  name: 'functionappsettings'
  properties: {
    BACKEND_API_URL: backendApiUrl
  }
}

output name string = staticWebApp.name
output uri string = 'https://${staticWebApp.properties.defaultHostname}'
output id string = staticWebApp.id
output defaultHostname string = staticWebApp.properties.defaultHostname