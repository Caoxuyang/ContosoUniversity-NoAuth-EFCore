---
name: dotnet-azure-storage-blob
description: Migrate file storage to Azure Storage Blob with Managed Identity. Use when migrating from AWS S3, local file system, or existing blob storage with connection strings/account keys to Azure Storage Blob with DefaultAzureCredential authentication.
---

# Azure Storage Blob

Contains samples, instructions, and other information that is useful for generating code that uses Azure Storage Blob dotnet SDK.

## Azure Blob Storage best practices

Follow these best practices when generating new Azure Blob Storage in .NET applications

- By default, choose AzureDefaultCredential + endpoint as the parameters to initialize the Azure Storage blob client. Managed Identity is the only recommended way, don't use connection string or account key to do authentication.
- SAS tokens are generated to delegate specific permissions (e.g., read, write, delete) for a given container or blob.
- These tokens are created using methods such as `BlobContainerClient.GenerateSasUri()` or `BlobServiceClient.GetUserDelegationKey()`, and include parameters like expiry time, permissions, and optionally IP restrictions.

## Azure Blob Storage Dependencies

```xml
<PackageReference Include="Azure.Storage.Blobs" Version="12.24.0" />
<PackageReference Include="Azure.Storage.Blobs.Batch" Version="12.21.0" />
<PackageReference Include="Azure.Identity" Version="1.14.0" />
```

## Sample Code

```json
{
    "AzureStorageBlob": {
        "Endpoint": "https://yourstorageaccount.blob.core.windows.net"
    }
}
```

```csharp
string endpoint =  builder.Configuration.GetValue<string>("AzureStorageBlob:Endpoint");

// Create BlobServiceClient using credential and endpoint
BlobServiceClient blobServiceClient = new BlobServiceClient(
    new Uri(endpoint),
    new DefaultAzureCredential());
```

## APIs for Azure Blob Storage:

> Note: Each API have sync & async versions as well as different signatures, take the proper one according to the codebase context.

### Class BlobServiceClient
- **Namespace**: Azure.Storage.Blobs
- **Description**: The BlobServiceClient allows you to manipulate Azure Storage service resources and blob containers.

**Constructors:**
- `BlobServiceClient(Uri serviceUri, TokenCredential credential, BlobClientOptions options = null)` - Creates a new instance using the specified endpoint, credential, and client options.
- `BlobServiceClient(string connectionString, BlobClientOptions options = null)` - Creates a new instance using a connection string.
- `BlobServiceClient(Uri serviceUri, StorageSharedKeyCredential credential)` - Creates a new instance with the endpoint and shared key credential.

**Key Methods:**
- `GetBlobContainerClient(string blobContainerName)` - Create a new BlobContainerClient object.
- `CreateBlobContainer(string blobContainerName, ...)` - Creates a new container under the specified account.
- `CreateBlobContainerAsync(string blobContainerName, ...)` - Creates a new container asynchronously.
- `GetUserDelegationKey(DateTimeOffset? startsOn, DateTimeOffset expiresOn, ...)` - Gets a user delegation key for use with this account's blob storage.
- `GetBlobContainers(...)` - Returns a collection of containers in this account.
- `FindBlobsByTags(string tagFilterSqlExpression, ...)` - Finds blobs whose tags match a given expression.
- `GenerateAccountSasUri(...)` - Generates a Blob Account Shared Access Signature (SAS).
- `DeleteBlobContainer(string blobContainerName, ...)` - Marks the specified blob container for deletion.
- `UndeleteBlobContainer(string deletedContainerName, string deletedContainerVersion, ...)` - Restores a previously deleted container.

### Class BlobClient
- **Namespace**: Azure.Storage.Blobs
- **Description**: The BlobClient allows you to manipulate Azure Storage blobs.

**Key Methods:**
- `Delete(...)` - Deletes the specified blob or snapshot.
- `DeleteIfExists(...)` - Deletes the specified blob or snapshot if it exists.
- `DownloadContent(...)` - Downloads a blob from the service, including its metadata and properties.
- `DownloadStreaming(...)` - Downloads a blob as a stream.
- `DownloadTo(Stream/string destination, ...)` - Downloads a blob using parallel requests.
- `Exists(...)` - Checks if the associated blob exists.
- `GenerateSasUri(...)` - Generates a Shared Access Signature for the blob.
- `GetProperties(...)` - Returns the blob's metadata and properties.
- `SetMetadata(IDictionary<string,string> metadata, ...)` - Sets the blob's metadata.
- `SetTags(IDictionary<string,string> tags, ...)` - Sets the blob's tags.
- `Upload(BinaryData/Stream/string content, ...)` - Uploads content to the blob.
- `StartCopyFromUri(Uri source, ...)` - Starts copying the blob at the source URL.
- `Undelete(...)` - Restores a soft deleted blob.

### Class BlobContainerClient
- **Namespace**: Azure.Storage.Blobs
- **Description**: The BlobContainerClient allows you to manipulate Azure Storage containers and their blobs.

**Key Methods:**
- `CreateIfNotExists(...)` - Creates a new container if it doesn't already exist.
- `Delete(...)` - Marks the specified container for deletion.
- `DeleteIfExists(...)` - Deletes the container if it exists.
- `Exists(...)` - Checks if the associated container exists.
- `GenerateSasUri(...)` - Generates a Container Service Shared Access Signature (SAS).
- `GetBlobClient(string blobName)` - Creates a new BlobClient object.
- `GetBlobs(...)` - Returns blobs in this container.
- `GetProperties(...)` - Returns container metadata and properties.
- `SetAccessPolicy(...)` - Sets the permissions for the container.
- `SetMetadata(...)` - Sets container metadata.
- `UploadBlob(string blobName, BinaryData/Stream content, ...)` - Creates a new block blob.

### Class BlobBatchClient
- **Namespace**: Azure.Storage.Blobs.Specialized
- **Package**: Azure.Storage.Blobs.Batch
- **Description**: A batch client for performing batch operations on Azure Blob Storage.

**Key Methods:**
- `DeleteBlobs(IEnumerable<Uri> blobUris, ...)` - Deletes multiple blobs in a single batched request.

### Class BlockBlobClient
- **Namespace**: Azure.Storage.Blobs.Specialized
- **Description**: The BlockBlobClient allows you to manipulate Azure Storage block blobs.

**Key Methods:**
- `AbortCopyFromUri(string copyId, ...)` - Aborts a pending copy operation.
- `CommitBlockList(IEnumerable<string> base64BlockIds, ...)` - Writes a blob by specifying the list of block IDs.
- `GetBlockList(...)` - Retrieves the list of blocks that have been uploaded.
- `StageBlock(string base64BlockId, Stream content, ...)` - Creates a new block as part of a block blob's staging area.

### Key Enums and Classes

**BlobSasPermissions** - Read, Write, Delete, List, Execute, PermanentDelete, All

**BlobContainerSasPermissions** - Read, Write, Delete, List, Execute, All

**PublicAccessType** - None, BlobContainer, Blob

**DeleteSnapshotsOption** - None, IncludeSnapshots, OnlySnapshots

**BlobUploadOptions** - Contains HttpHeaders, Metadata, Tags, Conditions, TransferOptions, AccessTier, etc.

**BlobDownloadOptions** - Contains Conditions and Range for downloading specific byte ranges.

**StorageErrorCode** - ContainerAlreadyExists, ContainerNotFound, BlobNotFound, InvalidBlockId, etc.

**HttpRange** - Represents a range in a HTTP resource for partial downloads.
