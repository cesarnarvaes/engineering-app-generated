@description('The name of the SQL server')
param serverName string

@description('The name of the SQL database')
param databaseName string

@description('The location for the SQL server and database')
param location string = resourceGroup().location

@description('Tags for the resources')
param tags object = {}

@description('The administrator username of the SQL logical server')
param administratorLogin string

@description('The administrator password of the SQL logical server')
@secure()
param administratorLoginPassword string

@description('The SKU name for the database')
param databaseSku string = 'S2'

@description('Allow Azure services to access server')
param allowAzureServices bool = true

@description('Enable Advanced Data Security')
param enableAdvancedDataSecurity bool = true

@description('Email address for security alerts')
param securityAlertEmail string = ''

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: serverName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// SQL Database
resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  tags: tags
  sku: {
    name: databaseSku
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 268435456000 // 250 GB
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Local'
    isLedgerOn: false
  }
}

// Firewall rule to allow Azure services
resource firewallRuleAllowAzureServices 'Microsoft.Sql/servers/firewallRules@2022-05-01-preview' = if (allowAzureServices) {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Advanced Data Security
resource advancedDataSecurity 'Microsoft.Sql/servers/securityAlertPolicies@2022-05-01-preview' = if (enableAdvancedDataSecurity) {
  parent: sqlServer
  name: 'default'
  properties: {
    state: 'Enabled'
    emailAddresses: securityAlertEmail != '' ? [securityAlertEmail] : []
    emailAccountAdmins: true
    retentionDays: 90
  }
}

// Auditing
resource auditing 'Microsoft.Sql/servers/auditingSettings@2022-05-01-preview' = {
  parent: sqlServer
  name: 'default'
  properties: {
    state: 'Enabled'
    isAzureMonitorTargetEnabled: true
    retentionDays: 30
  }
}

// Transparent Data Encryption
resource transparentDataEncryption 'Microsoft.Sql/servers/databases/transparentDataEncryption@2022-05-01-preview' = {
  parent: sqlDatabase
  name: 'current'
  properties: {
    state: 'Enabled'
  }
}

// Output values
output serverName string = sqlServer.name
output databaseName string = sqlDatabase.name
output connectionString string = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${databaseName};Authentication="Active Directory Default";Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
output serverFqdn string = sqlServer.properties.fullyQualifiedDomainName
output principalId string = sqlServer.identity.principalId
output tenantId string = sqlServer.identity.tenantId