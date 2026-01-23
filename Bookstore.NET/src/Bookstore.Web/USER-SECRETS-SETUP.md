# Local Development Setup - User Secrets

##  Configuration Complete

User Secrets have been initialized and configured with placeholder values. Your **appsettings.json** remains clean and safe to commit to source control.

##  Setting Your Real Azure AD Values

When you create your Azure AD app registration, update the secrets with your actual values:

```powershell
# Navigate to project directory
cd C:\git\PHP-Bookstore-Website-Example\Bookstore.NET\src\Bookstore.Web

# Set your real values (get these from Azure Portal)
dotnet user-secrets set "AzureAd:TenantId" "YOUR-ACTUAL-TENANT-ID"
dotnet user-secrets set "AzureAd:ClientId" "YOUR-ACTUAL-CLIENT-ID"
dotnet user-secrets set "AzureAd:Domain" "yourcompany.onmicrosoft.com"
```

##  View Current Secrets

```powershell
dotnet user-secrets list
```

##  Where Are Secrets Stored?

Secrets are stored locally on your machine at:
```
%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json
```

This location is:
-  Outside your project directory
-  Never committed to Git
-  Specific to your user account
-  Automatically loaded in Development environment

##  How It Works

1. In **Development** environment, .NET Core automatically loads User Secrets
2. User Secrets **override** values in appsettings.json
3. Your appsettings.json keeps placeholder values for documentation
4. For **Production** (Azure), use:
   - Azure Key Vault for secrets
   - Azure App Configuration
   - Or environment variables

##  Managing Secrets

```powershell
# List all secrets
dotnet user-secrets list

# Set a secret
dotnet user-secrets set "Key:Nested" "value"

# Remove a secret
dotnet user-secrets remove "Key:Nested"

# Clear all secrets
dotnet user-secrets clear
```

##  Creating Azure AD App Registration

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Click **New registration**
4. Name: `Bookstore-Dev`
5. Supported account types: **Single tenant**
6. Redirect URI: 
   - Platform: **Web**
   - URI: `https://localhost:5001/signin-oidc`
7. Click **Register**
8. Copy the **Application (client) ID**  Use for ClientId
9. Copy the **Directory (tenant) ID**  Use for TenantId
10. Note your **Directory domain**  Use for Domain

Then update your secrets:
```powershell
dotnet user-secrets set "AzureAd:TenantId" "<paste-tenant-id>"
dotnet user-secrets set "AzureAd:ClientId" "<paste-client-id>"
dotnet user-secrets set "AzureAd:Domain" "<your-domain>.onmicrosoft.com"
```

##  Current Configuration

Your current secrets (run `dotnet user-secrets list`):
-  ConnectionStrings:DefaultConnection  LocalDB
-  AzureAd:Domain  Placeholder
-  AzureAd:TenantId  Placeholder  
-  AzureAd:ClientId  Placeholder

**Status**: Ready for local development once you add real Azure AD values!
