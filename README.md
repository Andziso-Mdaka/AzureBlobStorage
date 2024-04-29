Azure Blob Storage Service
The Azure Blob Storage Service is a .NET library that provides a simple interface for interacting with Azure Blob Storage. It allows you to perform common operations such as uploading, downloading, listing, and deleting blobs in Azure Blob Storage.

Features
Upload Blob: Upload a file to a specified blob container in Azure Blob Storage.
List Blobs: List all blobs in a specified blob container.
Download Blob: Download a blob from a specified container to a local file.
Delete Blob: Delete a specified blob from a blob container.
Prerequisites
.NET Core 3.1 or later
An Azure Storage account with access to Blob Storage
Installation
To use the Azure Blob Storage Service in your .NET project, you can install it via NuGet Package Manager:

bash
dotnet add package Azure.Storage.Blobs
Usage
Initialize BlobStorageService:
csharp

string connectionString = "your_connection_string_here";
string containerName = "your_container_name_here";
BlobStorageService blobStorageService = new BlobStorageService(connectionString);
Perform Blob Operations:
csharp

// Upload Blob
await blobStorageService.UploadBlobAsync(containerName, filePath, blobName);

// List Blobs
List<string> blobNames = await blobStorageService.ListBlobsAsync(containerName);

// Download Blob
await blobStorageService.DownloadBlobAsync(containerName, blobName, downloadFilePath);

// Delete Blob
await blobStorageService.DeleteBlobAsync(containerName, blobName);
Logging
The BlobStorageService includes built-in logging capabilities to log successful and unsuccessful actions to a log container in Azure Blob Storage. You can customize the logging behavior by providing the connection string and log container name when initializing the BlobStorageService.
