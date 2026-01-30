// Azure Kubernetes Service Module
// Deploys AKS cluster with workload identity and OIDC issuer enabled

@description('Location for AKS')
param location string

@description('AKS cluster name')
param aksName string

@description('Kubernetes version')
param kubernetesVersion string = '1.29'

@description('VM size for nodes')
param nodeVmSize string = 'Standard_D2s_v3'

@description('Number of nodes')
@minValue(1)
@maxValue(10)
param nodeCount int = 2

@description('ACR resource ID for pull permissions')
param acrId string

@description('Enable container insights')
param enableMonitoring bool = true

@description('Log Analytics workspace ID')
param logAnalyticsWorkspaceId string = ''

resource aks 'Microsoft.ContainerService/managedClusters@2024-01-01' = {
  name: aksName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    dnsPrefix: aksName
    kubernetesVersion: kubernetesVersion
    enableRBAC: true
    
    // Enable OIDC issuer for workload identity
    oidcIssuerProfile: {
      enabled: true
    }
    
    // Enable workload identity
    securityProfile: {
      workloadIdentity: {
        enabled: true
      }
    }
    
    agentPoolProfiles: [
      {
        name: 'systempool'
        count: nodeCount
        vmSize: nodeVmSize
        osType: 'Linux'
        mode: 'System'
        enableAutoScaling: true
        minCount: 1
        maxCount: nodeCount * 2
      }
    ]
    
    networkProfile: {
      networkPlugin: 'azure'
      networkPolicy: 'azure'
      loadBalancerSku: 'standard'
    }
    
    addonProfiles: {
      omsagent: {
        enabled: enableMonitoring
        config: enableMonitoring ? {
          logAnalyticsWorkspaceResourceID: logAnalyticsWorkspaceId
        } : {}
      }
    }
  }
}

// Grant AKS pull access to ACR
resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aks.id, acrId, 'acrpull')
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: aks.properties.identityProfile.kubeletidentity.objectId
    principalType: 'ServicePrincipal'
  }
}

output aksName string = aks.name
output aksFqdn string = aks.properties.fqdn
output aksOidcIssuerUrl string = aks.properties.oidcIssuerProfile.issuerURL
output kubeletIdentityObjectId string = aks.properties.identityProfile.kubeletidentity.objectId
output aksIdentityPrincipalId string = aks.identity.principalId
