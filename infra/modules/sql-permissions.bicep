// sql-permissions.bicep
// NOTE: This is a reference implementation. Actual SQL permissions must be granted manually
// because Azure deployment scripts cannot authenticate to SQL with AAD from the deployment context.
// 
// MANUAL STEPS REQUIRED:
// 1. Deploy infrastructure with this commented out
// 2. Run the SQL commands via Azure Portal Query Editor:
//    - CREATE USER [managed-identity-name] FROM EXTERNAL PROVIDER;
//    - ALTER ROLE db_datareader ADD MEMBER [managed-identity-name];
//    - ALTER ROLE db_datawriter ADD MEMBER [managed-identity-name];
//    - ALTER ROLE db_ddladmin ADD MEMBER [managed-identity-name];
//
// For automation in CI/CD, use the Grant-SqlPermissions.ps1 script in a pipeline step
// after infrastructure deployment completes.

@description('Managed identity name to grant permissions to')
param managedIdentityName string

@description('SQL Server FQDN')
param sqlServerFqdn string

@description('SQL Database name')
param databaseName string

// This outputs the SQL commands that need to be run manually
output sqlCommands string = '''
-- Run these commands in Azure Portal Query Editor as SQL Admin:
USE ${databaseName};
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'${managedIdentityName}')
BEGIN
    CREATE USER [${managedIdentityName}] FROM EXTERNAL PROVIDER;
    PRINT 'Created user';
END

ALTER ROLE db_datareader ADD MEMBER [${managedIdentityName}];
ALTER ROLE db_datawriter ADD MEMBER [${managedIdentityName}];
ALTER ROLE db_ddladmin ADD MEMBER [${managedIdentityName}];
GO
'''
