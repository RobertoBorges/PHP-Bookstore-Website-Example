/*
  Managed Identity Module
  Creates user-assigned managed identity for AKS and application workload identity
*/

param location string
param identityName string
param tags object = {}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
  tags: tags
}

output identityId string = managedIdentity.id
output clientId string = managedIdentity.properties.clientId
output principalId string = managedIdentity.properties.principalId
output identityName string = managedIdentity.name
