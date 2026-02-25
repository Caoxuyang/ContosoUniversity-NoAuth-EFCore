---
name: dotnet-managed-identity
description: Migrate .NET applications to use Azure Managed Identity for authentication. Use when migrating from connection strings, client secrets, certificates, or credential-based auth to DefaultAzureCredential for Azure SQL, Storage, Key Vault, Service Bus, Event Hubs, Cosmos DB, and other Azure services.
---

# Managed Identity Guide for C# Projects

This document provides comprehensive guidance for migrating .NET applications to use Azure Managed Identity for authentication instead of connection strings, client secrets, certificates, or other credential-based authentication methods.

**Important**
Check the `dotnet-dependency-management` skill to know how to process .csproj file and manage dependencies.

## What is Managed Identity?

Managed Identity provides Azure services with an automatically managed identity in Azure Active Directory (now Microsoft Entra ID). This identity can be used to authenticate to any service that supports Azure AD authentication without having credentials in your code.

### Types of Managed Identity

1. **System-assigned Managed Identity**: Created as part of an Azure service instance
2. **User-assigned Managed Identity**: Created as a standalone Azure resource

## Managed Identity Best Practices

- **DO** use `DefaultAzureCredential` for local development and production environments
- **DO** use Managed Identity for authentication to Azure services whenever possible
- **DO NOT** store connection strings, client secrets, or certificates in your code or configuration files
- **DO** implement proper error handling and retry logic for credential acquisition
- **DO** use the latest versions of Azure SDK libraries that support Managed Identity
- **DO** configure proper RBAC (Role-Based Access Control) permissions for your Managed Identity

## Migration Scenarios and Patterns

### 1. Azure SQL Database Migration

**Before: Connection String Authentication**
```csharp
var connectionString = "Server=myserver.database.windows.net;Database=mydatabase;User Id=myuser;Password=mypassword;";
using var connection = new SqlConnection(connectionString);
```

**After: Managed Identity Authentication**

Dependencies:
```xml
<PackageReference Include="Azure.Identity" Version="1.14.0" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
```

Configuration (appsettings.json):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=myserver.database.windows.net;Database=mydatabase;Authentication=Active Directory Default;"
  }
}
```

Code:
```csharp
using Azure.Identity;
using Microsoft.Data.SqlClient;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
using var connection = new SqlConnection(connectionString);
```

### 2. Azure Storage Account Migration

**Before: Storage Account Key Authentication**
```csharp
var connectionString = "DefaultEndpointsProtocol=https;AccountName=mystorageaccount;AccountKey=myaccountkey;EndpointSuffix=core.windows.net";
var blobServiceClient = new BlobServiceClient(connectionString);
```

**After: Managed Identity Authentication**

Dependencies:
```xml
<PackageReference Include="Azure.Storage.Blobs" Version="12.24.0" />
<PackageReference Include="Azure.Identity" Version="1.14.0" />
```

Configuration (appsettings.json):
```json
{
  "AzureStorage": {
    "BlobEndpoint": "https://mystorageaccount.blob.core.windows.net"
  }
}
```

Code:
```csharp
using Azure.Identity;
using Azure.Storage.Blobs;

var endpoint = builder.Configuration.GetValue<string>("AzureStorage:BlobEndpoint");
var blobServiceClient = new BlobServiceClient(new Uri(endpoint), new DefaultAzureCredential());
```

### 3. Azure Key Vault Migration

**Before: Client Secret Authentication**
```csharp
var client = new SecretClient(new Uri(keyVaultUrl), new ClientSecretCredential(tenantId, clientId, clientSecret));
```

**After: Managed Identity Authentication**

Dependencies:
```xml
<PackageReference Include="Azure.Security.KeyVault.Secrets" Version="4.8.0" />
<PackageReference Include="Azure.Identity" Version="1.14.0" />
```

Code:
```csharp
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var keyVaultUrl = builder.Configuration.GetValue<string>("KeyVaultUrl");
var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
```

### 4. Azure Service Bus Migration

**Before: Connection String Authentication**
```csharp
var connectionString = "Endpoint=sb://myservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=mykey";
var client = new ServiceBusClient(connectionString);
```

**After: Managed Identity Authentication**

Dependencies:
```xml
<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.18.2" />
<PackageReference Include="Azure.Identity" Version="1.14.0" />
```

Configuration (appsettings.json):
```json
{
  "ServiceBus": {
    "FullyQualifiedNamespace": "myservicebus.servicebus.windows.net"
  }
}
```

Code:
```csharp
using Azure.Identity;
using Azure.Messaging.ServiceBus;

var fullyQualifiedNamespace = builder.Configuration.GetValue<string>("ServiceBus:FullyQualifiedNamespace");
var client = new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential());
```

### 5. Azure Event Hubs Migration

**Before: Connection String Authentication**
```csharp
var connectionString = "Endpoint=sb://myeventhub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=mykey";
var client = new EventHubProducerClient(connectionString, eventHubName);
```

**After: Managed Identity Authentication**

Dependencies:
```xml
<PackageReference Include="Azure.Messaging.EventHubs" Version="5.12.0" />
<PackageReference Include="Azure.Identity" Version="1.14.0" />
```

Code:
```csharp
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

var fullyQualifiedNamespace = "myeventhub.servicebus.windows.net";
var eventHubName = "myeventhub";
var client = new EventHubProducerClient(fullyQualifiedNamespace, eventHubName, new DefaultAzureCredential());
```

### 6. Azure Cosmos DB Migration

**Before: Master Key Authentication**
```csharp
var cosmosClient = new CosmosClient(endpoint, masterKey);
```

**After: Managed Identity Authentication**

Dependencies:
```xml
<PackageReference Include="Microsoft.Azure.Cosmos" Version="3.42.0" />
<PackageReference Include="Azure.Identity" Version="1.14.0" />
```

Code:
```csharp
using Azure.Identity;
using Microsoft.Azure.Cosmos;

var endpoint = builder.Configuration.GetValue<string>("CosmosDb:Endpoint");
var cosmosClient = new CosmosClient(endpoint, new DefaultAzureCredential());
```

## Error Handling and Troubleshooting

### Common Authentication Errors
```csharp
try
{
    var client = new BlobServiceClient(new Uri(endpoint), new DefaultAzureCredential());
    var containerClient = client.GetBlobContainerClient("mycontainer");
    await containerClient.GetPropertiesAsync();
}
catch (Azure.RequestFailedException ex) when (ex.Status == 401)
{
    // Handle authentication failure
    throw new UnauthorizedAccessException("Failed to authenticate with Azure services. Ensure Managed Identity is properly configured.", ex);
}
catch (Azure.RequestFailedException ex) when (ex.Status == 403)
{
    // Handle authorization failure
    throw new UnauthorizedAccessException("Access denied. Ensure the Managed Identity has the required permissions.", ex);
}
catch (CredentialUnavailableException ex)
{
    // Handle credential unavailable
    throw new InvalidOperationException("Azure credentials are not available. Ensure you're running on Azure or have Azure CLI/Visual Studio signed in.", ex);
}
```
