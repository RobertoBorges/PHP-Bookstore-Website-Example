/*
  RBAC Module
  Assigns role-based access control permissions to managed identity
  Uses resource IDs directly for cross-resource-group assignments

  ⚠️ NOTE: This module is NOT currently used in deployment due to cross-resource-group limitations.
  
  LIMITATION:
  - This deployment uses existing resources (Key Vault, ACR, SQL Server) in 'infraborges-rg' (East US)
  - New AKS cluster is deployed to a separate resource group in Canada Central
  - Bicep has limitations assigning roles across resource groups in a single deployment
  
  ACTUAL RBAC CONFIGURATION:
  All RBAC assignments were done manually via Azure CLI during deployment:
  1. ACR Pull: `az aks update --attach-acr nonprodborges`
  2. Key Vault Secrets User: `az role assignment create` with managed identity
  3. AKS RBAC Cluster Admin: `az role assignment create` with user principal
  4. SQL Database Permissions: Manual T-SQL commands in Azure Portal (see infra/modules/sql-permissions.bicep)
  
  USE CASE FOR THIS MODULE:
  - Can be used if ALL resources (AKS, Key Vault, ACR, SQL) are in the SAME resource group
  - Useful as documentation of required roles and permissions
  - Reference for manual RBAC configuration steps
*/

param managedIdentityPrincipalId string
param existingKeyVaultId string
param existingAcrId string
param existingSqlServerId string
param sqlDatabaseName string

// Parse resource names from IDs
var keyVaultName = last(split(existingKeyVaultId, '/'))
var acrName = last(split(existingAcrId, '/'))
var sqlServerName = last(split(existingSqlServerId, '/'))

// Built-in Role Definitions
var keyVaultSecretsUserRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
var acrPullRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
var sqlDbContributorRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '9b7fa17d-e63e-47b0-bb0a-15c516ac86ec')

// Reference existing resources (for same-RG deployments)
// Uncomment these when all resources are in the same resource group as this deployment
// resource existingKeyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
//   name: keyVaultName
// }
// resource existingAcr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' existing = {
//   name: acrName
// }
// resource existingSqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = {
//   name: sqlServerName
// }

// Key Vault Secrets User role assignment
// For same-RG: change scope to existingKeyVault
resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(existingKeyVaultId, managedIdentityPrincipalId, keyVaultSecretsUserRole)
  // For same-RG deployment, use: scope: existingKeyVault
  scope: any(existingKeyVaultId) // Workaround for cross-RG (not deployable, for reference only)
  properties: {
    roleDefinitionId: keyVaultSecretsUserRole
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ACR Pull role assignment
// For same-RG: change scope to existingAcr
resource acrRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(existingAcrId, managedIdentityPrincipalId, acrPullRole)
  // For same-RG deployment, use: scope: existingAcr
  scope: any(existingAcrId) // Workaround for cross-RG (not deployable, for reference only)
  properties: {
    roleDefinitionId: acrPullRole
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// SQL Server Contributor role assignment (assign to server, not database)
// For same-RG: change scope to existingSqlServer
resource sqlRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(existingSqlServerId, managedIdentityPrincipalId, sqlDbContributorRole)
  // For same-RG deployment, use: scope: existingSqlServer
  scope: any(existingSqlServerId) // Workaround for cross-RG (not deployable, for reference only)
  properties: {
    roleDefinitionId: sqlDbContributorRole
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output keyVaultRoleAssignmentId string = keyVaultRoleAssignment.id
output acrRoleAssignmentId string = acrRoleAssignment.id
output sqlRoleAssignmentId string = sqlRoleAssignment.id
