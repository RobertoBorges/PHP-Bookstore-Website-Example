# Infrastructure Deployment Conversion Log

**Date**: January 27, 2026  
**Phase**: Phase 4 - Infrastructure Generation  
**Change Type**: Architecture Decision - Kubernetes Deployment Strategy

---

## Decision: Helm Charts vs Plain YAML

### Context

During Phase 4 infrastructure generation, we initially created plain Kubernetes YAML files:
- namespace.yaml
- deployment.yaml  
- service.yaml
- ingress.yaml
- configmap.yaml
- secret.yaml

### Problem Identified

Plain YAML files have limitations for production deployments:
1. **No environment separation** - Same config for dev/staging/prod
2. **Hard-coded values** - Azure resource IDs embedded in YAML
3. **No versioning** - Can''t track or rollback deployments easily
4. **Duplication** - Need separate files per environment
5. **Maintenance burden** - Changes require editing multiple files

### Solution: Helm Charts

**Decision**: Convert to Helm chart architecture for production-grade deployment

**Benefits**:
-  Single chart with environment-specific values files
-  Parameterization of all Azure resource IDs from Bicep outputs
-  Built-in versioning and rollback capability (`helm rollback`)
-  Industry standard for AKS deployments
-  Seamless integration with Azure Developer CLI (`azd`)
-  Better secret management with Key Vault CSI Driver

### Implementation

**Helm Chart Structure Created**:

```
infra/helm/bookstore/
 Chart.yaml                    # Chart metadata v1.0.0
 values.yaml                   # Production baseline (3 replicas, 512Mi RAM)
 values.dev.yaml              # Dev overrides (1 replica, 128Mi RAM, Swagger on)
 values.prod.yaml             # Prod overrides (5 replicas, 1Gi RAM, rate limiting)
 README.md                    # Comprehensive Helm documentation
 templates/
     _helpers.tpl            # Helm template functions
     namespace.yaml          # Parameterized namespace
     serviceaccount.yaml     # Workload identity SA
     deployment.yaml         # Templated deployment spec
     service.yaml            # ClusterIP service template
     ingress.yaml            # Conditional NGINX ingress
     configmap.yaml          # Dynamic config from values
     secret.yaml             # Key Vault CSI Driver integration
```

### Key Features

**Environment Management**:
- **Development**: 1 replica, minimal resources, HTTP, Swagger enabled
- **Production**: 5 replicas, production resources, HTTPS, rate limiting, no Swagger

**Parameterization** (all from Bicep outputs):
- Azure Container Registry URL
- Managed Identity Client ID  
- Key Vault endpoint and name
- SQL Server FQDN
- Application Insights connection string
- Tenant ID

**Security**:
- Workload identity for Azure resource access
- Key Vault CSI Driver for secret management
- Non-root container execution
- Read-only root filesystem option

**Observability**:
- Health checks (liveness + readiness probes)
- Application Insights integration
- Resource limits and requests

### Deployment Commands

**Development**:
```powershell
helm install bookstore-dev ./infra/helm/bookstore -f infra/helm/bookstore/values.dev.yaml --namespace bookstore --create-namespace
```

**Production**:
```powershell
helm install bookstore ./infra/helm/bookstore -f infra/helm/bookstore/values.prod.yaml --namespace bookstore --create-namespace
```

**Upgrade**:
```powershell
helm upgrade bookstore ./infra/helm/bookstore -f infra/helm/bookstore/values.prod.yaml --namespace bookstore
```

**Rollback**:
```powershell
helm rollback bookstore --namespace bookstore
```

### Files Created

| File | Purpose | Lines |
|------|---------|-------|
| Chart.yaml | Chart metadata and versioning | 14 |
| values.yaml | Production baseline configuration | 115 |
| values.dev.yaml | Development overrides | 18 |
| values.prod.yaml | Production overrides | 25 |
| README.md | Helm chart documentation | 150 |
| templates/namespace.yaml | Namespace template | 10 |
| templates/serviceaccount.yaml | Workload identity SA | 12 |
| templates/deployment.yaml | Application deployment | 95 |
| templates/service.yaml | ClusterIP service | 15 |
| templates/ingress.yaml | NGINX ingress with TLS | 25 |
| templates/configmap.yaml | Non-sensitive config | 15 |
| templates/secret.yaml | Key Vault CSI integration | 45 |
| templates/_helpers.tpl | Helm helper functions | 50 |

**Total**: 13 files, ~589 lines

### Migration Impact

**Status Report Updates**:
- Task Group 3: Marked as converted to Helm chart
- Progress: Updated from 18 tasks to 25 tasks (added 7 Helm files)
- Completion: Updated to 88% (22/25 tasks complete)

**Next Steps**:
- Continue with Task Group 4: Deployment Scripts
- Update deployment scripts to use `helm install` instead of `kubectl apply`
- Document Helm chart usage in infrastructure README

### Trade-offs

**Pros**:
- Much more maintainable and scalable
- Industry best practice
- Better separation of concerns
- Version control and rollback
- Multi-environment support

**Cons**:
- Slight learning curve for Helm syntax
- More files to manage (but better organized)
- Need Helm CLI installed (widely available)

### Conclusion

Converting to Helm charts is the right decision for this project. It aligns with Azure best practices for AKS deployments, provides better environment management, and makes the infrastructure more maintainable and production-ready.
### Cleanup

**Old files deleted** (replaced by Helm templates):
- ❌ Deleted `infra/kubernetes/namespace.yaml`
- ❌ Deleted `infra/kubernetes/deployment.yaml`
- ❌ Deleted `infra/kubernetes/service.yaml`
- ❌ Deleted `infra/kubernetes/ingress.yaml`
- ❌ Deleted `infra/kubernetes/configmap.yaml`
- ❌ Deleted `infra/kubernetes/secret.yaml`
- ❌ Removed `infra/kubernetes/` directory

**Reason**: These plain YAML files are now obsolete. All Kubernetes resources are managed through the Helm chart at `infra/helm/bookstore/`.
---

**Approved by**: Migration Agent  
**Documented**: January 27, 2026  
**Status**:  Complete
