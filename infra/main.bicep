/*
  Main Bicep template for Bookstore .NET Application
  Target Region: Canada Central
  Deployment Model: Azure Kubernetes Service (AKS)
  
  This template creates the necessary infrastructure for the migrated PHP to .NET bookstore application.
  It references existing resources (SQL Server, Key Vault, ACR, Storage) and creates new AKS cluster,
  managed identities, Application Insights, and networking components.
*/

targetScope = 'resourceGroup'

// ========== Parameters ==========

@description('The environment name (e.g., dev, staging, prod)')
@allowed(['dev', 'staging', 'prod'])
param environmentName string = 'dev'

@description('The location/region for all resources')
param location string = 'canadacentral'

@description('The base name for all resources')
param resourceBaseName string = 'bookstore'

@description('Resource ID of the existing Azure SQL Server')
param existingSqlServerId string

@description('Name of the SQL database (will be created if not exists)')
param sqlDatabaseName string = 'BookstoreDB'

@description('Resource ID of the existing Key Vault')
param existingKeyVaultId string

@description('Resource ID of the existing Azure Container Registry')
param existingAcrId string

@description('Resource ID of the existing Storage Account')
param existingStorageAccountId string

@description('Azure AD Tenant ID for Entra ID integration')
param tenantId string = subscription().tenantId

@description('Azure AD Client ID for the application')
param aadClientId string

@description('Tags to apply to all resources')
param tags object = {
  Environment: environmentName
  Application: 'Bookstore'
  ManagedBy: 'Bicep'
  MigrationPhase: 'Phase4-Infrastructure'
}

@description('Enable Azure Monitor for containers')
param enableMonitoring bool = true

@description('Enable network policies (Calico)')
param enableNetworkPolicy bool = true

@description('Kubernetes version for AKS')
param kubernetesVersion string = '1.29.0'

// ========== Variables ==========

var resourceToken = toLower('${resourceBaseName}-${environmentName}-${uniqueString(resourceGroup().id)}')
var aksClusterName = '${resourceToken}-aks'
var managedIdentityName = '${resourceToken}-identity'
var logAnalyticsName = '${resourceToken}-logs'
var appInsightsName = '${resourceToken}-appins'
var vnetName = '${resourceToken}-vnet'

// Extract existing resource names from IDs
var existingKeyVaultName = last(split(existingKeyVaultId, '/'))
var existingAcrName = last(split(existingAcrId, '/'))
var existingSqlServerName = last(split(existingSqlServerId, '/'))

// ========== Modules ==========

// Managed Identity for AKS and application
module identity 'modules/identity.bicep' = {
  name: 'identity-deployment'
  params: {
    location: location
    identityName: managedIdentityName
    tags: tags
  }
}

// Virtual Network and Subnets
module network 'modules/networking.bicep' = {
  name: 'network-deployment'
  params: {
    location: location
    vnetName: vnetName
    tags: tags
  }
}

// Log Analytics Workspace
module logAnalytics 'modules/monitoring.bicep' = {
  name: 'monitoring-deployment'
  params: {
    location: location
    logAnalyticsName: logAnalyticsName
    appInsightsName: appInsightsName
    tags: tags
  }
}

// Azure Kubernetes Service (AKS) Cluster
module aks 'modules/aks.bicep' = {
  name: 'aks-deployment'
  params: {
    location: location
    clusterName: aksClusterName
    kubernetesVersion: kubernetesVersion
    managedIdentityId: identity.outputs.identityId
    vnetSubnetId: network.outputs.aksSubnetId
    logAnalyticsWorkspaceId: logAnalytics.outputs.workspaceId
    enableMonitoring: enableMonitoring
    enableNetworkPolicy: enableNetworkPolicy
    tenantId: tenantId
    aadClientId: aadClientId
    tags: tags
  }
}

// SQL Database (comment out - will create manually in existing SQL Server to avoid cross-RG deployment issues)
// module database 'modules/database.bicep' = {
//   name: 'database-deployment'
//   scope: resourceGroup(split(existingSqlServerId, '/')[2], split(existingSqlServerId, '/')[4])
//   params: {
//     location: location
//     sqlServerName: last(split(existingSqlServerId, '/'))
//     databaseName: sqlDatabaseName
//     tags: tags
//   }
// }

// RBAC Assignments (note: cross-resource-group RBAC requires manual assignment or separate deployment)
// Will be assigned via Azure CLI after deployment
// module rbac 'modules/rbac.bicep' = {
//   name: 'rbac-deployment'
//   params: {
//     managedIdentityPrincipalId: identity.outputs.principalId
//     existingKeyVaultId: existingKeyVaultId
//     existingAcrId: existingAcrId
//     existingSqlServerId: existingSqlServerId
//     sqlDatabaseName: sqlDatabaseName
//   }
// }

// ========== Outputs ==========

output aksClusterName string = aks.outputs.clusterName
output aksClusterId string = aks.outputs.clusterId
output aksFqdn string = aks.outputs.fqdn
output managedIdentityClientId string = identity.outputs.clientId
output managedIdentityId string = identity.outputs.identityId
output appInsightsInstrumentationKey string = logAnalytics.outputs.appInsightsInstrumentationKey
output appInsightsConnectionString string = logAnalytics.outputs.appInsightsConnectionString
output vnetId string = network.outputs.vnetId
output aksSubnetId string = network.outputs.aksSubnetId
// output databaseName string = database.outputs.databaseName
output acrName string = last(split(existingAcrId, '/'))
output keyVaultName string = last(split(existingKeyVaultId, '/'))
