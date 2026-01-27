# Infrastructure Pre-Deployment Validation Script
# This script validates all prerequisites before deploying infrastructure to Azure

param(
    [Parameter(Mandatory=$false)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName = "rg-bookstore-canadacentral",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "canadacentral"
)

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Bookstore Infrastructure Pre-Deployment Validation" -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""

$validationErrors = @()

# Function to test command availability
function Test-Command {
    param([string]$Command)
    try {
        Get-Command $Command -ErrorAction Stop | Out-Null
        return $true
    } catch {
        return $false
    }
}

# 1. Check Azure CLI
Write-Host " Checking Azure CLI..." -NoNewline
if (Test-Command "az") {
    $azVersion = az version --query '\"azure-cli\"' -o tsv
    Write-Host " Installed (v$azVersion)" -ForegroundColor Green
} else {
    Write-Host " NOT FOUND" -ForegroundColor Red
    $validationErrors += "Azure CLI is not installed. Install from: https://aka.ms/installazurecliwindows"
}

# 2. Check kubectl
Write-Host " Checking kubectl..." -NoNewline
if (Test-Command "kubectl") {
    $kubectlVersion = (kubectl version --client --short 2>$null).Split(' ')[2]
    Write-Host " Installed ($kubectlVersion)" -ForegroundColor Green
} else {
    Write-Host " NOT FOUND" -ForegroundColor Red
    $validationErrors += "kubectl is not installed. Install with: az aks install-cli"
}

# 3. Check Helm
Write-Host " Checking Helm..." -NoNewline
if (Test-Command "helm") {
    $helmVersion = (helm version --short 2>$null)
    Write-Host " Installed ($helmVersion)" -ForegroundColor Green
} else {
    Write-Host " NOT FOUND" -ForegroundColor Red
    $validationErrors += "Helm is not installed. Install from: https://helm.sh/docs/intro/install/"
}

# 4. Check Docker
Write-Host " Checking Docker..." -NoNewline
if (Test-Command "docker") {
    $dockerVersion = (docker --version).Split(',')[0]
    Write-Host " Installed ($dockerVersion)" -ForegroundColor Green
    
    # Check if Docker is running
    try {
        docker ps > $null 2>&1
        Write-Host "  Docker daemon is running" -ForegroundColor Green
    } catch {
        Write-Host "  WARNING: Docker daemon is not running" -ForegroundColor Yellow
        $validationErrors += "Docker daemon is not running. Start Docker Desktop."
    }
} else {
    Write-Host " NOT FOUND" -ForegroundColor Red
    $validationErrors += "Docker is not installed. Install from: https://www.docker.com/products/docker-desktop"
}

# 5. Check Azure login
Write-Host " Checking Azure authentication..." -NoNewline
$account = az account show 2>$null | ConvertFrom-Json
if ($account) {
    Write-Host " Logged in as $($account.user.name)" -ForegroundColor Green
    
    if ($SubscriptionId) {
        Write-Host "  Setting subscription to: $SubscriptionId" -ForegroundColor Cyan
        az account set --subscription $SubscriptionId
    } else {
        Write-Host "  Using subscription: $($account.name) ($($account.id))" -ForegroundColor Cyan
    }
} else {
    Write-Host " NOT LOGGED IN" -ForegroundColor Red
    $validationErrors += "Not logged into Azure. Run: az login"
}

# 6. Validate Bicep files
Write-Host " Validating Bicep templates..." -NoNewline
$bicepFiles = @(
    "infra\main.bicep",
    "infra\modules\identity.bicep",
    "infra\modules\networking.bicep",
    "infra\modules\monitoring.bicep",
    "infra\modules\aks.bicep",
    "infra\modules\database.bicep",
    "infra\modules\rbac.bicep"
)

$bicepValid = $true
foreach ($file in $bicepFiles) {
    if (-not (Test-Path $file)) {
        $bicepValid = $false
        $validationErrors += "Bicep file not found: $file"
    }
}

if ($bicepValid) {
    # Test bicep build
    az bicep build --file "infra\main.bicep" --stdout > $null 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host " Valid ($($bicepFiles.Count) files)" -ForegroundColor Green
    } else {
        Write-Host " SYNTAX ERRORS" -ForegroundColor Red
        $validationErrors += "Bicep template has syntax errors. Run: az bicep build --file infra\main.bicep"
    }
} else {
    Write-Host " MISSING FILES" -ForegroundColor Red
}

# 7. Validate Helm chart
Write-Host " Validating Helm chart..." -NoNewline
if (Test-Path "infra\helm\bookstore\Chart.yaml") {
    helm lint "infra\helm\bookstore" > $null 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host " Valid" -ForegroundColor Green
    } else {
        Write-Host " LINT ERRORS" -ForegroundColor Yellow
        $validationErrors += "Helm chart has lint warnings. Run: helm lint infra\helm\bookstore"
    }
} else {
    Write-Host " NOT FOUND" -ForegroundColor Red
    $validationErrors += "Helm chart not found at: infra\helm\bookstore"
}

# 8. Validate parameters file
Write-Host " Checking parameters file..." -NoNewline
if (Test-Path "infra\main.parameters.json") {
    $params = Get-Content "infra\main.parameters.json" | ConvertFrom-Json
    $placeholders = @()
    
    # Check for placeholder values
    $params.parameters.PSObject.Properties | ForEach-Object {
        if ($_.Value.value -match "<YOUR-.*>") {
            $placeholders += $_.Name
        }
    }
    
    if ($placeholders.Count -gt 0) {
        Write-Host " PLACEHOLDERS FOUND" -ForegroundColor Yellow
        Write-Host "  The following parameters need to be updated:" -ForegroundColor Yellow
        $placeholders | ForEach-Object { Write-Host "    - $_" -ForegroundColor Yellow }
    } else {
        Write-Host " Configured" -ForegroundColor Green
    }
} else {
    Write-Host " NOT FOUND" -ForegroundColor Red
    $validationErrors += "Parameters file not found: infra\main.parameters.json"
}

# 9. Check .NET application
Write-Host " Checking .NET application..." -NoNewline
if (Test-Path "Bookstore.NET\src\Bookstore.Web\Bookstore.Web.csproj") {
    Write-Host " Found" -ForegroundColor Green
} else {
    Write-Host " NOT FOUND" -ForegroundColor Red
    $validationErrors += ".NET project not found at: Bookstore.NET\src\Bookstore.Web"
}

# 10. Check resource group
Write-Host " Checking resource group..." -NoNewline
$rg = az group show --name $ResourceGroupName 2>$null | ConvertFrom-Json
if ($rg) {
    Write-Host " Exists ($($rg.location))" -ForegroundColor Green
} else {
    Write-Host " DOES NOT EXIST" -ForegroundColor Yellow
    Write-Host "  Will be created during deployment" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "==================================================================" -ForegroundColor Cyan

# Summary
if ($validationErrors.Count -eq 0) {
    Write-Host " VALIDATION PASSED" -ForegroundColor Green -BackgroundColor Black
    Write-Host "  All prerequisites are met. Ready to deploy!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Update infra\main.parameters.json with your Azure resource IDs" -ForegroundColor White
    Write-Host "  2. Run: .\infra\deploy.ps1" -ForegroundColor White
    Write-Host ""
    exit 0
} else {
    Write-Host " VALIDATION FAILED" -ForegroundColor Red -BackgroundColor Black
    Write-Host "  Please fix the following issues:" -ForegroundColor Red
    Write-Host ""
    $validationErrors | ForEach-Object { Write-Host "   $_" -ForegroundColor Red }
    Write-Host ""
    exit 1
}
