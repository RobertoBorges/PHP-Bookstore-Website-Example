/*
  AKS Cluster Module
  Creates Azure Kubernetes Service cluster with system node pool, monitoring, and Entra ID integration
*/

param location string
param clusterName string
param kubernetesVersion string = '1.29.0'
param managedIdentityId string
param vnetSubnetId string
param logAnalyticsWorkspaceId string
param enableMonitoring bool = true
param enableNetworkPolicy bool = true
param tenantId string
param aadClientId string
param tags object = {}

resource aksCluster 'Microsoft.ContainerService/managedClusters@2024-02-01' = {
  name: clusterName
  location: location
  tags: tags
  sku: {
    name: 'Base'
    tier: 'Premium'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    kubernetesVersion: kubernetesVersion
    dnsPrefix: clusterName
    enableRBAC: true
    aadProfile: {
      managed: true
      enableAzureRBAC: true
      tenantID: tenantId
      adminGroupObjectIDs: []
    }
    networkProfile: {
      networkPlugin: 'azure'
      networkPolicy: enableNetworkPolicy ? 'calico' : 'none'
      serviceCidr: '10.1.0.0/16'
      dnsServiceIP: '10.1.0.10'
      loadBalancerSku: 'standard'
      outboundType: 'loadBalancer'
    }
    agentPoolProfiles: [
      {
        name: 'systempool'
        count: 3
        vmSize: 'Standard_DS2_v2'
        osType: 'Linux'
        mode: 'System'
        vnetSubnetID: vnetSubnetId
        enableAutoScaling: true
        minCount: 2
        maxCount: 5
        maxPods: 30
        type: 'VirtualMachineScaleSets'
      }
    ]
    addonProfiles: enableMonitoring ? {
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: logAnalyticsWorkspaceId
        }
      }
    } : {}
    oidcIssuerProfile: {
      enabled: true
    }
    securityProfile: {
      workloadIdentity: {
        enabled: true
      }
    }
  }
}

output clusterName string = aksCluster.name
output clusterId string = aksCluster.id
output fqdn string = aksCluster.properties.fqdn
output nodeResourceGroup string = aksCluster.properties.nodeResourceGroup
output oidcIssuerUrl string = aksCluster.properties.oidcIssuerProfile.issuerURL
