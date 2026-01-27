# Infrastructure Deployment Script
# This script deploys the Bookstore application to Azure Kubernetes Service

param(
    [Parameter(Mandatory=$false)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName = "rg-bookstore-canadacentral",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "canadacentral",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("dev", "prod")]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$ImageTag = "latest",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipValidation,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipInfra,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipHelm
)

$ErrorActionPreference = "Stop"

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Bookstore Infrastructure Deployment" -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Environment: $Environment" -ForegroundColor White
Write-Host "  Location: $Location" -ForegroundColor White
Write-Host "  Resource Group: $ResourceGroupName" -ForegroundColor White
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Pre-deployment validation
if (-not $SkipValidation) {
    Write-Host "[Step 1/6] Running pre-deployment validation..." -ForegroundColor Cyan
    & ".\infra\validate.ps1" -SubscriptionId $SubscriptionId -ResourceGroupName $ResourceGroupName -Location $Location
    if ($LASTEXITCODE -ne 0) {
        Write-Host " Validation failed. Fix issues and try again." -ForegroundColor Red
        exit 1
    }
    Write-Host " Validation passed" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "[Step 1/6] Skipping validation (use -SkipValidation)" -ForegroundColor Yellow
    Write-Host ""
}

# Set subscription
if ($SubscriptionId) {
    Write-Host "Setting Azure subscription..." -ForegroundColor Cyan
    az account set --subscription $SubscriptionId
}

$currentSub = az account show | ConvertFrom-Json
Write-Host "Using subscription: $($currentSub.name)" -ForegroundColor White
Write-Host ""

# Step 2: Create resource group
Write-Host "[Step 2/6] Ensuring resource group exists..." -ForegroundColor Cyan
$rg = az group show --name $ResourceGroupName 2>$null | ConvertFrom-Json
if (-not $rg) {
    Write-Host "Creating resource group: $ResourceGroupName in $Location" -ForegroundColor White
    az group create --name $ResourceGroupName --location $Location | Out-Null
    Write-Host " Resource group created" -ForegroundColor Green
} else {
    Write-Host " Resource group already exists" -ForegroundColor Green
}
Write-Host ""

# Step 3: Deploy Bicep infrastructure
if (-not $SkipInfra) {
    Write-Host "[Step 3/6] Deploying Azure infrastructure (Bicep)..." -ForegroundColor Cyan
    Write-Host "This may take 10-15 minutes for AKS cluster provisioning..." -ForegroundColor Yellow
    
    $deploymentName = "bookstore-infra-$(Get-Date -Format 'yyyyMMddHHmmss')"
    
    az deployment group create `
        --name $deploymentName `
        --resource-group $ResourceGroupName `
        --template-file "infra\main.bicep" `
        --parameters "infra\main.parameters.json" `
        --parameters environmentName=$Environment `
        --verbose
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host " Infrastructure deployed successfully" -ForegroundColor Green
        
        # Get deployment outputs
        Write-Host "Retrieving deployment outputs..." -ForegroundColor Cyan
        $outputs = az deployment group show `
            --name $deploymentName `
            --resource-group $ResourceGroupName `
            --query properties.outputs `
            | ConvertFrom-Json
        
        $acrName = $outputs.acrName.value
        $aksName = $outputs.aksName.value
        $managedIdentityClientId = $outputs.managedIdentityClientId.value
        $keyVaultName = $outputs.keyVaultName.value
        
        Write-Host "  ACR: $acrName" -ForegroundColor White
        Write-Host "  AKS: $aksName" -ForegroundColor White
        Write-Host "  Key Vault: $keyVaultName" -ForegroundColor White
        Write-Host "  Managed Identity: $managedIdentityClientId" -ForegroundColor White
    } else {
        Write-Host " Infrastructure deployment failed" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "[Step 3/6] Skipping infrastructure deployment (-SkipInfra)" -ForegroundColor Yellow
    
    # Try to get existing resources
    Write-Host "Looking for existing resources..." -ForegroundColor Cyan
    $resources = az resource list --resource-group $ResourceGroupName | ConvertFrom-Json
    $acrResource = $resources | Where-Object { $_.type -eq "Microsoft.ContainerRegistry/registries" } | Select-Object -First 1
    $aksResource = $resources | Where-Object { $_.type -eq "Microsoft.ContainerService/managedClusters" } | Select-Object -First 1
    
    if ($acrResource) { $acrName = $acrResource.name }
    if ($aksResource) { $aksName = $aksResource.name }
}
Write-Host ""

# Step 4: Build and push Docker image
if (-not $SkipBuild) {
    Write-Host "[Step 4/6] Building and pushing Docker image..." -ForegroundColor Cyan
    
    if (-not $acrName) {
        Write-Host " ACR name not found. Cannot build image." -ForegroundColor Red
        exit 1
    }
    
    # Login to ACR
    Write-Host "Logging into Azure Container Registry..." -ForegroundColor Cyan
    az acr login --name $acrName
    
    $acrLoginServer = az acr show --name $acrName --query loginServer -o tsv
    $imageName = "$acrLoginServer/bookstore-web:$ImageTag"
    
    Write-Host "Building Docker image: $imageName" -ForegroundColor White
    docker build -t $imageName -f Dockerfile .
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host " Image built successfully" -ForegroundColor Green
        
        Write-Host "Pushing image to ACR..." -ForegroundColor Cyan
        docker push $imageName
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host " Image pushed to ACR" -ForegroundColor Green
        } else {
            Write-Host " Failed to push image" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host " Docker build failed" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "[Step 4/6] Skipping Docker build (-SkipBuild)" -ForegroundColor Yellow
}
Write-Host ""

# Step 5: Get AKS credentials
Write-Host "[Step 5/6] Configuring kubectl for AKS..." -ForegroundColor Cyan

if (-not $aksName) {
    Write-Host " AKS name not found. Cannot configure kubectl." -ForegroundColor Red
    exit 1
}

az aks get-credentials --resource-group $ResourceGroupName --name $aksName --overwrite-existing
if ($LASTEXITCODE -eq 0) {
    Write-Host " kubectl configured" -ForegroundColor Green
    
    # Test connection
    Write-Host "Testing AKS connection..." -ForegroundColor Cyan
    $nodes = kubectl get nodes -o json 2>$null | ConvertFrom-Json
    if ($nodes.items.Count -gt 0) {
        Write-Host " Connected to AKS ($($nodes.items.Count) nodes)" -ForegroundColor Green
    } else {
        Write-Host " Warning: Could not verify AKS nodes" -ForegroundColor Yellow
    }
} else {
    Write-Host " Failed to get AKS credentials" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 6: Deploy application with Helm
if (-not $SkipHelm) {
    Write-Host "[Step 6/6] Deploying application with Helm..." -ForegroundColor Cyan
    
    $valuesFile = "infra\helm\bookstore\values.$Environment.yaml"
    $releaseName = if ($Environment -eq "dev") { "bookstore-dev" } else { "bookstore" }
    
    # Check if release exists
    $existingRelease = helm list -n bookstore -o json 2>$null | ConvertFrom-Json | Where-Object { $_.name -eq $releaseName }
    
    if ($existingRelease) {
        Write-Host "Upgrading existing Helm release: $releaseName" -ForegroundColor White
        helm upgrade $releaseName ".\infra\helm\bookstore" `
            -f $valuesFile `
            --namespace bookstore `
            --wait `
            --timeout 10m
    } else {
        Write-Host "Installing new Helm release: $releaseName" -ForegroundColor White
        helm install $releaseName ".\infra\helm\bookstore" `
            -f $valuesFile `
            --namespace bookstore `
            --create-namespace `
            --wait `
            --timeout 10m
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host " Application deployed successfully" -ForegroundColor Green
    } else {
        Write-Host " Helm deployment failed" -ForegroundColor Red
        Write-Host "Check logs with: kubectl logs -n bookstore -l app=bookstore-web" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "[Step 6/6] Skipping Helm deployment (-SkipHelm)" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host " DEPLOYMENT COMPLETE" -ForegroundColor Green -BackgroundColor Black
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Check pod status: kubectl get pods -n bookstore" -ForegroundColor White
Write-Host "  2. View logs: kubectl logs -n bookstore -l app=bookstore-web --tail=50 -f" -ForegroundColor White
Write-Host "  3. Get ingress IP: kubectl get ingress -n bookstore" -ForegroundColor White
Write-Host "  4. Check Helm release: helm status $releaseName -n bookstore" -ForegroundColor White
Write-Host ""
Write-Host "Rollback if needed:" -ForegroundColor Cyan
Write-Host "  helm rollback $releaseName -n bookstore" -ForegroundColor White
Write-Host ""
