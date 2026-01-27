# Bookstore Infrastructure as Code

This directory contains all Infrastructure as Code (IaC) for deploying the Bookstore .NET 10 application to Azure Kubernetes Service (AKS).

##  Directory Structure

```
infra/
 main.bicep                    # Main orchestration template
 main.parameters.json          # Deployment parameters
 validate.ps1                  # Pre-deployment validation script
 deploy.ps1                    # Automated deployment script
 modules/                      # Bicep modules
    identity.bicep           # Managed identity
    networking.bicep         # VNet and subnets
    monitoring.bicep         # Log Analytics + App Insights
    aks.bicep                # AKS cluster configuration
    database.bicep           # Azure SQL Database
    rbac.bicep               # Role assignments
 helm/bookstore/              # Helm chart for Kubernetes deployment
     Chart.yaml               # Chart metadata
     values.yaml              # Production baseline
     values.dev.yaml          # Development overrides
     values.prod.yaml         # Production overrides
     README.md                # Helm chart documentation
     templates/               # Kubernetes manifests
         namespace.yaml
         serviceaccount.yaml
         deployment.yaml
         service.yaml
         ingress.yaml
         configmap.yaml
         secret.yaml
```

##  Quick Start

### Prerequisites

1. **Azure CLI** - [Install](https://aka.ms/installazurecliwindows)
2. **kubectl** - `az aks install-cli`
3. **Helm 3** - [Install](https://helm.sh/docs/intro/install/)
4. **Docker Desktop** - [Install](https://www.docker.com/products/docker-desktop)
5. **Azure Subscription** with permissions to create resources

### Step 1: Configure Parameters

Edit `main.parameters.json` and replace all `<YOUR-*>` placeholders with your actual Azure resource IDs:

```json
{
  "existingSqlServerId": "/subscriptions/.../resourceGroups/.../providers/Microsoft.Sql/servers/your-sql-server",
  "existingKeyVaultId": "/subscriptions/.../resourceGroups/.../providers/Microsoft.KeyVault/vaults/your-keyvault",
  "existingAcrId": "/subscriptions/.../resourceGroups/.../providers/Microsoft.ContainerRegistry/registries/your-acr",
  "existingStorageAccountId": "/subscriptions/.../resourceGroups/.../providers/Microsoft.Storage/storageAccounts/your-storage"
}
```

### Step 2: Validate Prerequisites

Run the validation script to check all prerequisites:

```powershell
.\infra\validate.ps1
```

This checks:
-  Azure CLI, kubectl, Helm, Docker installed
-  Logged into Azure
-  Bicep templates are valid
-  Helm chart is valid
-  Parameters file exists

### Step 3: Deploy Infrastructure

**Development environment:**

```powershell
.\infra\deploy.ps1 -Environment dev
```

**Production environment:**

```powershell
.\infra\deploy.ps1 -Environment prod
```

**With custom options:**

```powershell
.\infra\deploy.ps1 `
    -SubscriptionId "your-subscription-id" `
    -ResourceGroupName "rg-bookstore-canadacentral" `
    -Location "canadacentral" `
    -Environment "prod" `
    -ImageTag "v1.0.0"
```

##  What Gets Deployed

### Azure Resources (Bicep)

| Resource | Purpose | Configuration |
|----------|---------|---------------|
| **User-Assigned Managed Identity** | Secure Azure resource access | Workload identity enabled |
| **Virtual Network** | Network isolation | 10.0.0.0/16 with 3 subnets |
| **Log Analytics Workspace** | Centralized logging | 30-day retention |
| **Application Insights** | APM and monitoring | Workspace-based |
| **AKS Cluster** | Kubernetes hosting | 3 nodes (Standard_DS2_v2), auto-scale 2-5 |
| **Azure SQL Database** | Application database | Basic tier, 2GB |
| **Role Assignments** | RBAC permissions | Key Vault, ACR, SQL access |

### Kubernetes Resources (Helm)

| Resource | Purpose | Dev Config | Prod Config |
|----------|---------|------------|-------------|
| **Namespace** | Resource isolation | bookstore | bookstore |
| **ServiceAccount** | Workload identity | Enabled | Enabled |
| **Deployment** | Application pods | 1 replica, 128Mi RAM | 5 replicas, 512Mi RAM |
| **Service** | Internal load balancing | ClusterIP | ClusterIP |
| **Ingress** | External access | HTTP | HTTPS + rate limiting |
| **ConfigMap** | Non-sensitive config | Swagger enabled | Swagger disabled |
| **Secret** | Key Vault integration | CSI Driver | CSI Driver |

##  Deployment Script Options

The `deploy.ps1` script supports several options:

```powershell
# Skip validation (faster for repeat deployments)
.\infra\deploy.ps1 -SkipValidation

# Skip infrastructure (only deploy app changes)
.\infra\deploy.ps1 -SkipInfra

# Skip Docker build (use existing image)
.\infra\deploy.ps1 -SkipBuild

# Skip Helm deployment (only provision infrastructure)
.\infra\deploy.ps1 -SkipHelm

# Custom image tag
.\infra\deploy.ps1 -ImageTag "v1.2.3"
```

##  Security Configuration

### Secrets Management

This infrastructure uses **Azure Key Vault CSI Driver** for secure secret management:

1. Secrets are stored in Azure Key Vault
2. AKS pods mount secrets via CSI driver
3. Managed identity authenticates to Key Vault
4. No secrets in code or Kubernetes etcd

**Required Key Vault Secrets:**

- `ConnectionStrings--DefaultConnection` - Azure SQL connection string with AAD auth
- `ApplicationInsights--ConnectionString` - Application Insights connection string

### Authentication

- **Workload Identity**: Enabled for secure Azure resource access
- **Microsoft Entra ID**: RBAC for AKS cluster and SQL Database
- **Managed Identity**: For Key Vault, ACR, and SQL Database access

##  Post-Deployment Configuration

### SQL Database Permissions (REQUIRED)

⚠️ **Manual Step Required**: After infrastructure deployment, you must grant SQL permissions to the managed identity.

**Why Manual?** Bicep cannot execute T-SQL commands. This step authenticates the managed identity with Azure SQL Database.

**Steps:**

1. Navigate to **Azure Portal** → **SQL Database** → **BookstoreDB** → **Query Editor**
2. Sign in as an **Azure AD admin**
3. Run these SQL commands:

```sql
-- Create user for managed identity
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'<managed-identity-name>')
BEGIN
    CREATE USER [<managed-identity-name>] FROM EXTERNAL PROVIDER;
END

-- Grant permissions
ALTER ROLE db_datareader ADD MEMBER [<managed-identity-name>];
ALTER ROLE db_datawriter ADD MEMBER [<managed-identity-name>];
ALTER ROLE db_ddladmin ADD MEMBER [<managed-identity-name>];
GO
```

**Automated Alternative for CI/CD:**

Use the provided PowerShell script:

```powershell
.\infra\scripts\Grant-SqlPermissions.ps1 `
  -SqlServerName "your-sql-server" `
  -DatabaseName "BookstoreDB" `
  -ManagedIdentityName "your-managed-identity-name"
```

> **Note**: The script requires `Invoke-Sqlcmd` or Azure Portal access. Azure CLI v2.x doesn't support direct SQL execution.

After granting permissions, restart the application pods:

```powershell
kubectl delete pod -n bookstore -l app=bookstore-web
```

##  Monitoring and Observability

### Application Insights

Automatic telemetry collection for:
- HTTP requests and dependencies
- Exceptions and traces
- Custom metrics
- Live metrics stream

### Log Analytics

Centralized logs from:
- AKS cluster (Container Insights)
- Application pods
- Azure resources
- Network and security events

**Query logs:**

```powershell
az monitor log-analytics query `
  --workspace <workspace-id> `
  --analytics-query "ContainerLog | where ContainerName == 'bookstore-web' | order by TimeGenerated desc | limit 100"
```

##  Upgrade and Rollback

### Upgrade Application

```powershell
# Build new version
docker build -t <acr>.azurecr.io/bookstore-web:v2.0.0 .
docker push <acr>.azurecr.io/bookstore-web:v2.0.0

# Update Helm release
helm upgrade bookstore ./infra/helm/bookstore `
  -f infra/helm/bookstore/values.prod.yaml `
  --set image.tag=v2.0.0 `
  --namespace bookstore
```

### Rollback

```powershell
# View release history
helm history bookstore -n bookstore

# Rollback to previous version
helm rollback bookstore -n bookstore

# Rollback to specific revision
helm rollback bookstore 5 -n bookstore
```

##  Troubleshooting

### Check Pod Status

```powershell
kubectl get pods -n bookstore
kubectl describe pod <pod-name> -n bookstore
kubectl logs <pod-name> -n bookstore --tail=100 -f
```

### Check Helm Release

```powershell
helm status bookstore -n bookstore
helm get values bookstore -n bookstore
```

### Validate Bicep Templates

```powershell
az bicep build --file infra/main.bicep
az deployment group validate `
  --resource-group rg-bookstore `
  --template-file infra/main.bicep `
  --parameters infra/main.parameters.json
```

### Validate Helm Chart

```powershell
helm lint infra/helm/bookstore
helm template bookstore infra/helm/bookstore -f infra/helm/bookstore/values.dev.yaml
```

### Check AKS Diagnostics

```powershell
# Node status
kubectl get nodes
kubectl describe node <node-name>

# Resource usage
kubectl top nodes
kubectl top pods -n bookstore

# Events
kubectl get events -n bookstore --sort-by='.lastTimestamp'
```

### Secret Mounting Issues

```powershell
# Check SecretProviderClass
kubectl get secretproviderclass -n bookstore
kubectl describe secretproviderclass bookstore-keyvault-secrets -n bookstore

# Check mounted secrets
kubectl exec -n bookstore <pod-name> -- ls -la /mnt/secrets-store

# Check workload identity
kubectl describe serviceaccount bookstore-sa -n bookstore
```

##  Additional Resources

- [Azure Kubernetes Service Documentation](https://docs.microsoft.com/azure/aks/)
- [Helm Documentation](https://helm.sh/docs/)
- [Azure Bicep Documentation](https://docs.microsoft.com/azure/azure-resource-manager/bicep/)
- [Azure Key Vault CSI Driver](https://azure.github.io/secrets-store-csi-driver-provider-azure/)
- [Workload Identity](https://learn.microsoft.com/azure/aks/workload-identity-overview)

##  Cleanup

To delete all resources:

```powershell
# Uninstall Helm release
helm uninstall bookstore -n bookstore

# Delete resource group (WARNING: Deletes ALL resources)
az group delete --name rg-bookstore-canadacentral --yes --no-wait
```

##  Notes

- **Region**: Default is Canada Central (`canadacentral`)
- **Cost**: AKS cluster with 3 Standard_DS2_v2 nodes  $200-300/month
- **Scaling**: AKS auto-scales from 2-5 nodes based on load
- **Backups**: Configure Azure SQL automated backups separately
- **DNS**: Update Helm values with your actual domain for ingress
- **TLS**: Install cert-manager for Let''s Encrypt certificates

---

**Generated**: Phase 4 - Infrastructure Generation  
**Last Updated**: January 27, 2026  
**Version**: 1.0.0
