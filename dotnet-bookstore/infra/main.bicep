// Azure Developer CLI (azd) Compatible Template
// Deploys: AKS + Monitoring
// Uses EXISTING: ACR, SQL Server, Key Vault

targetScope = 'resourceGroup'

// ============================================================================
// PARAMETERS (azd provides these automatically)
// ============================================================================

@description('Location for all resources')
param location string = resourceGroup().location

@description('Environment name (from azd)')
param environmentName string

@description('Principal ID for role assignments')
param principalId string = ''

// AKS Configuration
@description('AKS node count')
@minValue(1)
@maxValue(10)
param aksNodeCount int = 2

@description('AKS node VM size')
param aksNodeVmSize string = 'Standard_D2s_v3'

@description('Kubernetes version')
param kubernetesVersion string = '1.29'

// Existing Resources - You must provide these
@description('Resource ID of your existing Azure Container Registry')
param existingAcrResourceId string

@description('Name of your existing ACR (for login server)')
param existingAcrName string

@description('Connection string for existing SQL Database')
@secure()
param sqlConnectionString string

@description('Your domain for ingress (e.g., bookstore.yourdomain.com)')
param ingressHost string = ''

// ============================================================================
// VARIABLES
// ============================================================================

var resourceToken = toLower(uniqueString(resourceGroup().id, environmentName))
var aksName = 'aks-${resourceToken}'
var logAnalyticsName = 'log-${resourceToken}'
var appInsightsName = 'ai-${resourceToken}'

// ============================================================================
// MODULES
// ============================================================================

// Monitoring
module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring-deployment'
  params: {
    location: location
    logAnalyticsName: logAnalyticsName
    appInsightsName: appInsightsName
    retentionInDays: 30
  }
}

// AKS Cluster
module aks 'modules/aks.bicep' = {
  name: 'aks-deployment'
  params: {
    location: location
    aksName: aksName
    kubernetesVersion: kubernetesVersion
    nodeVmSize: aksNodeVmSize
    nodeCount: aksNodeCount
    acrId: existingAcrResourceId
    enableMonitoring: true
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsId
  }
}

// ============================================================================
// OUTPUTS (required by azd)
// ============================================================================

output AZURE_LOCATION string = location
output AZURE_AKS_NAME string = aks.outputs.aksName
output AZURE_AKS_FQDN string = aks.outputs.aksFqdn
output AZURE_AKS_OIDC_ISSUER string = aks.outputs.aksOidcIssuerUrl
output AZURE_AKS_KUBELET_IDENTITY string = aks.outputs.kubeletIdentityObjectId

output AZURE_CONTAINER_REGISTRY_NAME string = existingAcrName
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = '${existingAcrName}.azurecr.io'

output APPLICATIONINSIGHTS_CONNECTION_STRING string = monitoring.outputs.appInsightsConnectionString

output SQL_CONNECTION_STRING string = sqlConnectionString
output INGRESS_HOST string = ingressHost
