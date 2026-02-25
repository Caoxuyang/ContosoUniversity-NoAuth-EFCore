---
name: dotnet-azure-keyvault-secret
description: Migrate secrets to Azure Key Vault. Use when migrating hard-coded credentials, connection strings, API keys, secrets in configuration files (appsettings.json, Web.config), secrets in environment variables, or secrets from third-party managers (AWS Secrets Manager, HashiCorp Vault) to Azure Key Vault with DefaultAzureCredential.
---

# Azure Key Vault Secret integration in .NET

Azure Key Vault securely stores secrets such as connection strings, API keys, and credentials. This document provides comprehensive guidance, code samples, and best practices for integrating Azure Key Vault secret management into .NET applications, covering authentication, configuration, and secret operations.

**Important**
Check the `dotnet-managed-identity` skill for detailed guidance on how to leverage Managed Identity during the migration.

## Azure Key Vault best practices

Follow these best practices when generating new Azure Key Vault in .NET applications.

- **DO** use the modern Azure SDK (`Azure.Security.KeyVault.Secrets`) when the .NET version is .NET Standard 2.0 compatible.
- **DO** use DefaultAzureCredential for authentication wherever possible. It supports multiple environments (CLI, Visual Studio, Managed Identity) without needing code changes.
- **DO** create separate keyvault service class to manage secrets.
- **Never** hard-code any secrets in the codebase or configuration files, even in development environments.
- **DO NOT** use client secret or client cert to authenticate with Azure Key Vault.

## .NET configuration management best practices

- Use the built-in configuration management system in .NET to manage application settings.
- Never hard-code any secrets in the codebase or configuration files, even in development environments.

## .NET Version-Specific Guidance for Azure Key Vault Integration

### Dependency Management

- For SDK-style projects in `csproj` file:
```xml
<!-- For SDK-style projects -->
<ItemGroup>
  <PackageReference Include="Azure.Security.KeyVault.Secrets" Version="4.8.0" />
  <PackageReference Include="Azure.Identity" Version="1.14.0" />
</ItemGroup>
```

- For legacy .NET Framework projects using `packages.config`:

```xml
<!-- For traditional .NET Framework projects using packages.config -->
<packages>
  <package id="Azure.Security.KeyVault.Secrets" version="4.8.0" targetFramework="net462" />
  <package id="Azure.Identity" version="1.14.0" targetFramework="net462" />
</packages>
```

### Configuration Management

- Configuration in file `App.config` or `Web.config` for legacy .NET framework projects:
  ```xml
  <configuration>
    <appSettings>
      <add key="KeyVaultUri" value="https://your-vault-name.vault.azure.net/" />
      <!-- other configuration key values -->
    </appSettings>
  </configuration>
  ```

- Configuration in `appsettings.json` or `appsettings.{Environment}.json` for .NET Core / .NET 5+ projects:
    ```json
    {
      "KeyVaultName": "your-vault-name",
       // other configuration key values
    }
    ```

### Sample Code

- Build client
```csharp
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

// Create a SecretClient using DefaultAzureCredential
var client = new SecretClient(
    new Uri(System.Configuration.ConfigurationManager.AppSettings["KeyVaultUri"]),
    new DefaultAzureCredential());
```

- Get secret
```csharp
KeyVaultSecret secret = await client.GetSecretAsync("secret-name");
string secretValue = secret.Value;
```

- List secrets
```csharp
AsyncPageable<SecretProperties> allSecrets = client.GetPropertiesOfSecretsAsync();

await foreach (SecretProperties secretProperties in allSecrets)
{
    Console.WriteLine(secretProperties.Name);
}
```

- Update an existing secret
```csharp
KeyVaultSecret secret = await client.GetSecretAsync("secret-name");
// Clients may specify the content type of a secret to assist in interpreting the secret data when its retrieved.
secret.Properties.ContentType = "text/plain";
// You can specify additional application-specific metadata in the form of tags.
secret.Properties.Tags["foo"] = "updated tag";
SecretProperties updatedSecretProperties = await client.UpdateSecretPropertiesAsync(secret.Properties);
```

- Delete and purge a secret
```csharp
DeleteSecretOperation operation = await client.StartDeleteSecretAsync("secret-name");

// You only need to wait for completion if you want to purge or recover the secret.
await operation.WaitForCompletionAsync();

DeletedSecret secret = operation.Value;
await client.PurgeDeletedSecretAsync(secret.Name);
```

- **Sample code for configuration integration (recommended for ASP.NET Core)**:
```csharp
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential(),
    new AzureKeyVaultConfigurationOptions
    {
        // Optional: Configure reload interval, key prefix, etc.
    });

// Access secrets directly from configuration
var mySecret = builder.Configuration["mySecretName"];
```

### Exception handling
When you interact with the Azure Key Vault secrets client library using the .NET SDK, errors returned by the service correspond to the same HTTP status codes returned for REST API requests. And can be caught with the `RequestFailedException` exception.
