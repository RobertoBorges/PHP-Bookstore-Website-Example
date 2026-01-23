# Azure AD App Registration Setup Guide

## Prerequisites
- Azure subscription with Azure AD tenant
- Permissions to register applications in Azure AD
- Azure CLI or Azure Portal access

## Step 1: Register Application in Azure AD

### Using Azure Portal:

1. **Navigate to Azure Portal**: https://portal.azure.com
2. **Go to Azure Active Directory** > **App registrations** > **New registration**
3. **Configure application**:
   - **Name**: `Bookstore-Web-App`
   - **Supported account types**: Select one of:
     - `Accounts in this organizational directory only` (Single tenant)
     - `Accounts in any organizational directory` (Multi-tenant)
   - **Redirect URI**: 
     - Platform: `Web`
     - URI: `https://localhost:5001/signin-oidc` (for local testing)
     - Add additional URIs for production deployment later
4. **Click Register**

### Using Azure CLI:

```powershell
# Login to Azure
az login

# Create app registration
az ad app create --display-name "Bookstore-Web-App" \
  --web-redirect-uris "https://localhost:5001/signin-oidc" \
  --sign-in-audience "AzureADMyOrg"

# Note the appId (ClientId) from the output
```

## Step 2: Configure Authentication

1. **In the app registration**, go to **Authentication**
2. **Add platform** > **Web** (if not already added)
3. **Redirect URIs**: Add the following:
   - `https://localhost:5001/signin-oidc` (local development)
   - `https://localhost:5001/signout-oidc`
   - Add production URLs when deploying to Azure
4. **Front-channel logout URL**: `https://localhost:5001/signout-oidc`
5. **Implicit grant and hybrid flows**: 
   -  Check **ID tokens** (used for implicit and hybrid flows)
6. **Save changes**

## Step 3: Gather Required Information

From your Azure AD app registration, collect:

1. **Application (client) ID**: Found on the Overview page
   - Example: `12345678-1234-1234-1234-123456789abc`

2. **Directory (tenant) ID**: Found on the Overview page
   - Example: `87654321-4321-4321-4321-cba987654321`

3. **Domain**: Your Azure AD domain
   - Example: `contoso.onmicrosoft.com`
   - Or custom domain: `contoso.com`

## Step 4: Update appsettings.json

Replace the placeholders in `appsettings.json`:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "YOUR_DOMAIN.onmicrosoft.com",         Replace with your domain
    "TenantId": "YOUR_TENANT_ID_HERE",                Replace with Directory (tenant) ID
    "ClientId": "YOUR_CLIENT_ID_HERE",                Replace with Application (client) ID
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-oidc"
  }
}
```

### Example (do NOT use these values):
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "contoso.onmicrosoft.com",
    "TenantId": "87654321-4321-4321-4321-cba987654321",
    "ClientId": "12345678-1234-1234-1234-123456789abc",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-oidc"
  }
}
```

## Step 5: Configure API Permissions (Optional)

If you need to call Microsoft Graph API:

1. Go to **API permissions** in your app registration
2. Click **Add a permission** > **Microsoft Graph** > **Delegated permissions**
3. Select permissions needed:
   - `User.Read` (Read user profile)
   - `email` (View users' email address)
   - `openid` (Sign users in)
   - `profile` (View users' basic profile)
4. Click **Add permissions**
5. **Grant admin consent** if required by your organization

## Step 6: Add Users/Test Accounts

For testing, you need users in your Azure AD tenant:

### Option A: Existing corporate users
- Users with accounts in your Azure AD can sign in immediately

### Option B: Create test user
1. Go to **Azure Active Directory** > **Users** > **New user**
2. Create user:
   - **User principal name**: testuser@yourdomain.onmicrosoft.com
   - **Display name**: Test User
   - Set password (select "Show password" and copy it)
3. Click **Create**

## Step 7: Test Locally

```powershell
cd C:\git\PHP-Bookstore-Website-Example\Bookstore.NET\src\Bookstore.Web
dotnet run
```

Browse to: `https://localhost:5001`
- Click **Login** in the navigation bar
- Sign in with your Azure AD credentials
- Test the application functionality

## Step 8: Production Deployment

When deploying to Azure, add production redirect URIs:

1. In Azure Portal, go to your app registration
2. **Authentication** > **Add URI**
3. Add production URIs:
   - `https://your-app-name.azurewebsites.net/signin-oidc`
   - `https://your-app-name.azurewebsites.net/signout-oidc`
   - Or custom domain URIs for AKS deployment

## Troubleshooting

### Error: AADSTS50011 - Reply URL mismatch
- Ensure redirect URI in Azure AD exactly matches your application URL
- Check for trailing slashes, http vs https

### Error: AADSTS700016 - Application not found
- Verify ClientId in appsettings.json matches Azure AD app registration
- Ensure you're using the correct tenant

### Users can't sign in
- Check if user exists in Azure AD tenant
- Verify "Supported account types" setting matches your use case
- Check if admin consent is required for your organization

## Security Best Practices

1. **Never commit credentials**: Keep TenantId and ClientId in:
   - User Secrets (local development): `dotnet user-secrets set "AzureAd:ClientId" "your-value"`
   - Azure Key Vault (production)
   
2. **Use managed identities**: For Azure-to-Azure connections

3. **Restrict redirect URIs**: Only add URIs you control

4. **Enable Conditional Access**: Add MFA requirements in Azure AD

5. **Monitor sign-ins**: Use Azure AD sign-in logs for auditing

## Quick Setup Script (PowerShell)

```powershell
# Run this after creating app registration to update config
$tenantId = "YOUR_TENANT_ID"
$clientId = "YOUR_CLIENT_ID"
$domain = "YOUR_DOMAIN.onmicrosoft.com"

dotnet user-secrets set "AzureAd:TenantId" "$tenantId"
dotnet user-secrets set "AzureAd:ClientId" "$clientId"
dotnet user-secrets set "AzureAd:Domain" "$domain"
```

## Next Steps

After authentication is configured:
- Proceed with Phase 4: Infrastructure Generation
- Deploy to Azure Kubernetes Service (AKS)
- Set up CI/CD pipelines
- Configure production database

