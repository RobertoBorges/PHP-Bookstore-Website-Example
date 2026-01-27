/*
  Networking Module
  Creates VNet with subnets for AKS, ingress, and private endpoints
*/

param location string
param vnetName string
param tags object = {}

resource vnet 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  name: vnetName
  location: location
  tags: tags
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: 'aks-subnet'
        properties: {
          addressPrefix: '10.0.0.0/20'
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
      {
        name: 'ingress-subnet'
        properties: {
          addressPrefix: '10.0.16.0/24'
        }
      }
      {
        name: 'private-endpoints-subnet'
        properties: {
          addressPrefix: '10.0.17.0/24'
          privateEndpointNetworkPolicies: 'Disabled'
        }
      }
    ]
  }
}

output vnetId string = vnet.id
output vnetName string = vnet.name
output aksSubnetId string = '${vnet.id}/subnets/aks-subnet'
output ingressSubnetId string = '${vnet.id}/subnets/ingress-subnet'
output privateEndpointSubnetId string = '${vnet.id}/subnets/private-endpoints-subnet'
