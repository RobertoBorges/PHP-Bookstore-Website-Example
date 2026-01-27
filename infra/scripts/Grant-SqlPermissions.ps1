# Grant-SqlPermissions.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$SqlServerName,
    [Parameter(Mandatory=$true)]
    [string]$DatabaseName,
    [Parameter(Mandatory=$true)]
    [string]$ManagedIdentityName
)

Write-Host "`nGranting SQL permissions to managed identity..." -ForegroundColor Cyan

$sqlScript = @"
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$ManagedIdentityName')
BEGIN
    CREATE USER [$ManagedIdentityName] FROM EXTERNAL PROVIDER;
    PRINT 'User created';
END
ELSE
    PRINT 'User exists';

ALTER ROLE db_datareader ADD MEMBER [$ManagedIdentityName];
ALTER ROLE db_datawriter ADD MEMBER [$ManagedIdentityName];
ALTER ROLE db_ddladmin ADD MEMBER [$ManagedIdentityName];
PRINT 'Permissions granted';
"@

$tempFile = [System.IO.Path]::GetTempFileName()
$sqlScript | Out-File -FilePath $tempFile -Encoding UTF8

try {
    Write-Host "Executing SQL on $SqlServerName/$DatabaseName..."
    az sql db execute --server $SqlServerName --database $DatabaseName --file $tempFile --auth-type default
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n Permissions granted!" -ForegroundColor Green
    } else {
        Write-Host "`n Failed" -ForegroundColor Red
        exit 1
    }
} finally {
    Remove-Item $tempFile -ErrorAction SilentlyContinue
}
