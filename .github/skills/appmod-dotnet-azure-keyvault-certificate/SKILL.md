---
name: dotnet-azure-keyvault-certificate
description: Migrate certificates to Azure Key Vault Certificate management with Managed Identity. Use when managing SSL/TLS certificates, code signing certificates, X.509 certificates stored locally or in file systems, with DefaultAzureCredential authentication.
---

# Azure Key Vault Certificate integration in .NET

Azure Key Vault securely stores and manages certificates including SSL/TLS certificates, code signing certificates, and other digital certificates. This document provides comprehensive guidance, code samples, and best practices for integrating Azure Key Vault certificate management into .NET applications, covering authentication, configuration, and certificate operations.

**Important**
Check the `dotnet-managed-identity` skill for detailed guidance on how to leverage Managed Identity during the migration.

## Azure Key Vault best practices

Follow these best practices when generating new Azure Key Vault in .NET applications.

- **DO** use the modern Azure SDK (`Azure.Security.KeyVault.Certificates`) when the .NET version is .NET Standard 2.0 compatible.
- **DO** use DefaultAzureCredential for authentication wherever possible. It supports multiple environments (CLI, Visual Studio, Managed Identity) without needing code changes.
- **DO** create separate keyvault service class to manage certificate.
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
  <PackageReference Include="Azure.Security.KeyVault.Certificates" Version="4.6.0" />
  <PackageReference Include="Azure.Identity" Version="1.14.0" />
</ItemGroup>
```

- For legacy .NET Framework projects using `packages.config`:

```xml
<!-- For traditional .NET Framework projects using packages.config -->
<packages>
  <package id="Azure.Security.KeyVault.Certificates" version="4.6.0" targetFramework="net462" />
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
using Azure.Security.KeyVault.Certificates;
using Azure.Identity;

// Create a CertificateClient using DefaultAzureCredential
var client = new CertificateClient(
    new Uri(System.Configuration.ConfigurationManager.AppSettings["KeyVaultUri"]),
    new DefaultAzureCredential());
```

- Create a certificate asynchronously
```csharp
// Create a certificate. This starts a long running operation to create and sign the certificate.
CertificateOperation operation = await client.StartCreateCertificateAsync("MyCertificate", CertificatePolicy.Default);

// You can await the completion of the create certificate operation.
KeyVaultCertificateWithPolicy certificate = await operation.WaitForCompletionAsync();
```

- Retrieve a certificate asynchronously
```csharp
KeyVaultCertificateWithPolicy certificateWithPolicy = await client.GetCertificateAsync("MyCertificate");
KeyVaultCertificate certificate = await client.GetCertificateVersionAsync(certificateWithPolicy.Name, certificateWithPolicy.Properties.Version);
```

- Update an existing certificate asynchronously
```csharp
CertificateProperties certificateProperties = new CertificateProperties(certificate.Id);
certificateProperties.Tags["key1"] = "value1";

KeyVaultCertificate updated = await client.UpdateCertificatePropertiesAsync(certificateProperties);
```

- List certificates asynchronously
```csharp
AsyncPageable<CertificateProperties> allCertificates = client.GetPropertiesOfCertificatesAsync();

await foreach (CertificateProperties certificateProperties in allCertificates)
{
    Console.WriteLine(certificateProperties.Name);
}
```

- Delete a certificate asynchronously
```csharp
DeleteCertificateOperation operation = await client.StartDeleteCertificateAsync("MyCertificate");

// You only need to wait for completion if you want to purge or recover the certificate.
await operation.WaitForCompletionAsync();

DeletedCertificate certificate = operation.Value;
await client.PurgeDeletedCertificateAsync(certificate.Name);
```

### Exception handling
When you interact with the Azure Key Vault certificates client library using the .NET SDK, errors returned by the service correspond to the same HTTP status codes returned for REST API requests. And can be caught with the `RequestFailedException` exception.
