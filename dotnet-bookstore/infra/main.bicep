// Main Bicep template for Bookstore .NET Application
// Target: Azure Kubernetes Service with Azure SQL Database

targetScope = ''resourceGroup''

@description(''Location for all resources'')
param location string = resourceGroup().location

@description(''Base name for resources'')
param baseName string = ''bookstore''

@description(''Environment name (dev, staging, prod)'')
@allowed([''dev'', ''staging'', ''prod''])
param environment string = ''dev''

@description(''SQL Server administrator login'')
param sqlAdminLogin string = ''sqladmin''

@secure()
@description(''SQL Server administrator password'')
param sqlAdminPassword string

@description(''Application Insights instrumentation key'')
param appInsightsConnectionString string = ''''

// Resource naming
var resourcePrefix = ''${baseName}-${environment}''
var sqlServerName = ''${resourcePrefix}-sql''
var sqlDatabaseName = ''BookstoreDb''
var acrName = replace(''${baseName}${environment}acr'', ''-'', '''')
var appInsightsName = ''${resourcePrefix}-insights''
var keyVaultName = ''${resourcePrefix}-kv''

// Azure Container Registry
resource acr ''Microsoft.ContainerRegistry/registries@2023-07-01'' = {
  name: acrName
  location: location
  sku: {
    name: ''Standard''
  }
  properties: {
    adminUserEnabled: false
  }
}

// Azure SQL Server
resource sqlServer ''Microsoft.Sql/servers@2023-05-01-preview'' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: ''1.2''
    publicNetworkAccess: ''Enabled''
  }
}

// Azure SQL Database
resource sqlDatabase ''Microsoft.Sql/servers/databases@2023-05-01-preview'' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: ''Basic''
    tier: ''Basic''
  }
  properties: {
    collation: ''SQL_Latin1_General_CP1_CI_AS''
    maxSizeBytes: 2147483648
  }
}

// Allow Azure Services firewall rule
resource sqlFirewallAzure ''Microsoft.Sql/servers/firewallRules@2023-05-01-preview'' = {
  parent: sqlServer
  name: ''AllowAzureServices''
  properties: {
    startIpAddress: ''0.0.0.0''
    endIpAddress: ''0.0.0.0''
  }
}

// Application Insights
resource appInsights ''Microsoft.Insights/components@2020-02-02'' = {
  name: appInsightsName
  location: location
  kind: ''web''
  properties: {
    Application_Type: ''web''
    publicNetworkAccessForIngestion: ''Enabled''
    publicNetworkAccessForQuery: ''Enabled''
  }
}

// Key Vault for secrets
resource keyVault ''Microsoft.KeyVault/vaults@2023-07-01'' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: ''A''
      name: ''standard''
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
  }
}

// Store SQL connection string in Key Vault
resource sqlConnectionStringSecret ''Microsoft.KeyVault/vaults/secrets@2023-07-01'' = {
  parent: keyVault
  name: ''SqlConnectionString''
  properties: {
    value: ''Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;''
  }
}

// Outputs
output acrLoginServer string = acr.properties.loginServer
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output keyVaultUri string = keyVault.properties.vaultUri
