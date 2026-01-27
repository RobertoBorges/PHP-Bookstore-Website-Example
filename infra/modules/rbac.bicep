/*
  RBAC Module
  Assigns role-based access control permissions to managed identity
  Uses resource IDs directly for cross-resource-group assignments
*/

param managedIdentityPrincipalId string
param existingKeyVaultId string
param existingAcrId string
param existingSqlServerId string
param sqlDatabaseName string

// Built-in Role Definitions
var keyVaultSecretsUserRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
var acrPullRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
var sqlDbContributorRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '9b7fa17d-e63e-47b0-bb0a-15c516ac86ec')

// Key Vault Secrets User role assignment (using resource ID)
resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(existingKeyVaultId, managedIdentityPrincipalId, keyVaultSecretsUserRole)
  scope: resourceId(split(existingKeyVaultId, '/')[2], split(existingKeyVaultId, '/')[4], 'Microsoft.KeyVault/vaults', last(split(existingKeyVaultId, '/')))
  properties: {
    roleDefinitionId: keyVaultSecretsUserRole
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ACR Pull role assignment (using resource ID)
resource acrRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(existingAcrId, managedIdentityPrincipalId, acrPullRole)
  scope: resourceId(split(existingAcrId, '/')[2], split(existingAcrId, '/')[4], 'Microsoft.ContainerRegistry/registries', last(split(existingAcrId, '/')))
  properties: {
    roleDefinitionId: acrPullRole
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// SQL Server Contributor role assignment (assign to server, not database)
resource sqlRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(existingSqlServerId, managedIdentityPrincipalId, sqlDbContributorRole)
  scope: resourceId(split(existingSqlServerId, '/')[2], split(existingSqlServerId, '/')[4], 'Microsoft.Sql/servers', last(split(existingSqlServerId, '/')))
  properties: {
    roleDefinitionId: sqlDbContributorRole
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output keyVaultRoleAssignmentId string = keyVaultRoleAssignment.id
output acrRoleAssignmentId string = acrRoleAssignment.id
output sqlRoleAssignmentId string = sqlRoleAssignment.id
