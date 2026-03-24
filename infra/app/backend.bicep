@description('Name of the Container App')
param name string

@description('Location for the Container App')
param location string = resourceGroup().location

@description('Tags for the resources')
param tags object = {}

@description('Container Apps Environment ID')
param containerAppsEnvironmentId string

@description('Container Registry name')
param containerRegistryName string

@description('Application Insights Connection String')
param applicationInsightsConnectionString string = ''

@description('Key Vault name')
param keyVaultName string = ''

@description('SQL Database connection string')
param sqlConnectionString string

@description('JWT Secret Key')
@secure()
param jwtSecretKey string

@description('Does the service already exist')
param exists bool = false

@description('Container image name')
param containerImage string = 'bizcrm-backend:latest'

@description('CPU allocation')
param containerCpu string = '0.5'

@description('Memory allocation')
param containerMemory string = '1Gi'

@description('Minimum replicas')
param minReplicas int = 1

@description('Maximum replicas')
param maxReplicas int = 10

// Get existing Container Registry
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: containerRegistryName
}

// Container App
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        allowInsecure: false
        traffic: [
          {
            weight: 100
            latestRevision: true
          }
        ]
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: 'system'
        }
      ]
      secrets: [
        {
          name: 'jwt-secret'
          value: jwtSecretKey
        }
        {
          name: 'sql-connection-string'
          value: sqlConnectionString
        }
      ]
    }
    template: {
      revisionSuffix: 'v${utcNow('yyyyMMdd-HHmmss')}'
      containers: [
        {
          image: exists ? '${containerRegistry.properties.loginServer}/${containerImage}' : 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
          name: 'backend-api'
          resources: {
            cpu: json(containerCpu)
            memory: containerMemory
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'sql-connection-string'
            }
            {
              name: 'JWT__SecretKey'
              secretRef: 'jwt-secret'
            }
            {
              name: 'JWT__Issuer'
              value: 'BizCrmAPI'
            }
            {
              name: 'JWT__Audience'
              value: 'BizCrmClient'
            }
            {
              name: 'JWT__ExpiryMinutes'
              value: '60'
            }
            {
              name: 'AZURE_KEY_VAULT_URL'
              value: !empty(keyVaultName) ? 'https://${keyVaultName}.vault.azure.net/' : ''
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: applicationInsightsConnectionString
            }
          ]
          probes: [
            {
              type: 'Readiness'
              httpGet: {
                path: '/health'
                port: 8080
              }
              initialDelaySeconds: 30
              periodSeconds: 10
              timeoutSeconds: 5
              failureThreshold: 3
            }
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 8080
              }
              initialDelaySeconds: 60
              periodSeconds: 30
              timeoutSeconds: 10
              failureThreshold: 3
            }
          ]
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
        rules: [
          {
            name: 'http-requests'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
          {
            name: 'cpu-utilization'
            custom: {
              type: 'cpu'
              metadata: {
                type: 'Utilization'
                value: '70'
              }
            }
          }
          {
            name: 'memory-utilization'
            custom: {
              type: 'memory'
              metadata: {
                type: 'Utilization'
                value: '80'
              }
            }
          }
        ]
      }
    }
  }
}

output name string = containerApp.name
output uri string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output id string = containerApp.id
output principalId string = containerApp.identity.principalId