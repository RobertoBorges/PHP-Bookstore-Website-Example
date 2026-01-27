# Bookstore Helm Chart

This Helm chart deploys the Bookstore .NET 10 application to Azure Kubernetes Service (AKS).

## Prerequisites

- Helm 3.x installed
- kubectl configured for your AKS cluster
- Azure resources deployed (via Bicep templates in `infra/`)
- Container image built and pushed to Azure Container Registry

## Installation

### 1. Update values file with your Azure resources

Edit `values.yaml` and replace placeholders:
- `<YOUR-ACR-NAME>` - Your Azure Container Registry name
- `<MANAGED-IDENTITY-CLIENT-ID>` - From Bicep output
- `<YOUR-KEY-VAULT-ENDPOINT>` - Your Key Vault URL
- `<YOUR-SQL-SERVER-FQDN>` - Your SQL Server FQDN
- `<YOUR-APP-INSIGHTS-CONNECTION-STRING>` - Application Insights connection string
- `<YOUR-KEY-VAULT-NAME>` - Key Vault name
- `<YOUR-TENANT-ID>` - Azure tenant ID

### 2. Install the chart

**Development environment:**
```powershell
helm install bookstore-dev . -f values.dev.yaml --namespace bookstore --create-namespace
```

**Production environment:**
```powershell
helm install bookstore . -f values.prod.yaml --namespace bookstore --create-namespace
```

### 3. Verify deployment

```powershell
helm status bookstore --namespace bookstore
kubectl get pods -n bookstore
kubectl get svc -n bookstore
kubectl get ingress -n bookstore
```

## Upgrade

```powershell
# Development
helm upgrade bookstore-dev . -f values.dev.yaml --namespace bookstore

# Production
helm upgrade bookstore . -f values.prod.yaml --namespace bookstore
```

## Uninstall

```powershell
helm uninstall bookstore --namespace bookstore
```

## Configuration

The following table lists the configurable parameters:

| Parameter | Description | Default |
|-----------|-------------|---------|
| `replicaCount` | Number of replicas | `3` |
| `image.registry` | Container registry | `<YOUR-ACR-NAME>.azurecr.io` |
| `image.repository` | Image repository | `bookstore-web` |
| `image.tag` | Image tag | `latest` |
| `resources.limits.cpu` | CPU limit | `500m` |
| `resources.limits.memory` | Memory limit | `512Mi` |
| `service.type` | Service type | `ClusterIP` |
| `ingress.enabled` | Enable ingress | `true` |
| `ingress.host` | Ingress hostname | `bookstore.yourdomain.com` |
| `secrets.strategy` | Secret strategy | `keyvault-csi` |

See `values.yaml` for all available parameters.

## Environments

- **values.dev.yaml** - Development configuration (1 replica, lower resources, Swagger enabled)
- **values.prod.yaml** - Production configuration (5 replicas, higher resources, rate limiting)

## Secret Management

This chart supports two secret management strategies:

1. **Azure Key Vault CSI Driver** (Recommended for production)
   - Set `secrets.strategy: keyvault-csi`
   - Requires Key Vault secrets to be pre-created
   - Uses workload identity for authentication

2. **Kubernetes Secrets** (Development only)
   - Set `secrets.strategy: kubernetes-secret`
   - Secrets stored in Kubernetes etcd
   - Not recommended for production

## Troubleshooting

### Check pod logs
```powershell
kubectl logs -n bookstore -l app=bookstore-web --tail=100 -f
```

### Check pod events
```powershell
kubectl describe pod -n bookstore -l app=bookstore-web
```

### Validate Helm chart
```powershell
helm lint .
helm template bookstore . -f values.yaml
```

### Debug secret mounting
```powershell
kubectl exec -n bookstore -it <pod-name> -- ls -la /mnt/secrets-store
```
