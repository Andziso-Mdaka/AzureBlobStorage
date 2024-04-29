using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class BlobStorageService
{
    private BlobServiceClient _blobServiceClient;
    private BlobContainerClient _logContainerClient;
    //
    public BlobStorageService(string connectionString, string logContainerName)
    {
        _blobServiceClient = new BlobServiceClient(connectionString);
        _logContainerClient = _blobServiceClient.GetBlobContainerClient(logContainerName);
    }

    private async Task LogAsync(string message)
    {
        string logFileName = $"log_{DateTime.UtcNow.ToString("yyyyMMdd")}.txt";
        BlobClient logBlobClient = _logContainerClient.GetBlobClient(logFileName);

        // Write log message to the log blob
        using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(message)))
        {
            await logBlobClient.UploadAsync(stream, overwrite: true);
        }
    }

    private async Task LogSuccessAsync(string action, string containerName, string blobName)
    {
        string message = $"[SUCCESS] Action: {action}, Container: {containerName}, Blob: {blobName}, Timestamp: {DateTime.UtcNow}";
        await LogAsync(message);
    }

    private async Task LogErrorAsync(string action, string containerName, string blobName, string errorMessage)
    {
        string message = $"[ERROR] Action: {action}, Container: {containerName}, Blob: {blobName}, Error: {errorMessage}, Timestamp: {DateTime.UtcNow}";
        await LogAsync(message);
    }


    public async Task UploadBlobAsync(string containerName, string filePath, string blobName)
    {
        try
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(filePath, overwrite: true);

            await LogSuccessAsync("UploadBlob", containerName, blobName); // Log success
            Console.WriteLine($"File '{filePath}' uploaded to blob '{blobName}' successfully.");
        }
        catch (RequestFailedException ex)
        {
            await LogErrorAsync("UploadBlob", containerName, blobName, ex.Message); // Log error
            Console.WriteLine($"Error uploading file to blob: {ex.Message}");
        }
        catch (Exception ex)
        {
            await LogErrorAsync("UploadBlob", containerName, blobName, ex.Message); // Log error
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<List<string>> ListBlobsAsync(string containerName)
    {
        List<string> blobNames = new List<string>();

        // Get a BlobContainerClient object to represent the specified container
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        // List all blobs in the container
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            blobNames.Add(blobItem.Name);
        }

        return blobNames;
    }

    public async Task DownloadBlobAsync(string containerName, string blobName, string downloadFilePath, bool overwrite = false)
    {
        try
        {

            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            if (!overwrite && File.Exists(downloadFilePath))
            {
                throw new IOException("File already exists and overwrite is not enabled.");
            }

            await blobClient.DownloadToAsync(downloadFilePath);

            Console.WriteLine($"Blob '{blobName}' downloaded to '{downloadFilePath}' successfully.");
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"Error downloading blob: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task DeleteBlobAsync(string containerName, string blobName)
    {
        try
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                throw new InvalidOperationException($"Blob '{blobName}' does not exist.");
            }

            await blobClient.DeleteAsync();

            Console.WriteLine($"Blob '{blobName}' deleted successfully.");
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"Error deleting blob: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        string connectionString = "DefaultEndpointsProtocol=https;AccountName=azurestorageaccount12;AccountKey=WLCNRIqDtfM7Ty8CUlncJ6lnwhuPyMFPQBslBICXOr+PqiU9YFf7CcRLY3efv2QUZVaqDYhw8OAZ+ASt8s5mHA==;EndpointSuffix=core.windows.net";
        string containerName = "pictrues";
        string logContainerName = "logs";

        BlobStorageService blobStorageService = new BlobStorageService(connectionString, logContainerName);

        while (true)
        {
            Console.WriteLine("Select an option:");
            Console.WriteLine("1. List Blobs");
            Console.WriteLine("2. Upload Blob");
            Console.WriteLine("3. Download Blob");
            Console.WriteLine("4. Delete Blob");
            Console.WriteLine("5. Exit");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine("Listing blobs...");
                    List<string> blobNames = await blobStorageService.ListBlobsAsync(containerName);
                    foreach (string blob in blobNames)
                    {
                        Console.WriteLine(blob);
                    }
                    break;

                case "2":
                    Console.WriteLine("Enter the file path to upload:");
                    string filePath = Console.ReadLine();
                    Console.WriteLine("Enter the blob name:");
                    string blobName = Console.ReadLine();
                    Console.WriteLine("Uploading blob...");
                    await blobStorageService.UploadBlobAsync(containerName, filePath, blobName);
                    break;
                case "3":
                    Console.WriteLine("Enter the blob name to download:");
                    string downloadBlobName = Console.ReadLine();
                    Console.WriteLine("Enter the file path to save the downloaded blob:");
                    string downloadFilePath = Console.ReadLine();
                    Console.WriteLine("Downloading blob...");
                    await blobStorageService.DownloadBlobAsync(containerName, downloadBlobName, downloadFilePath);
                    break;
                case "4":
                    Console.WriteLine("Enter the blob name to delete:");
                    string deleteBlobName = Console.ReadLine();
                    Console.WriteLine("Deleting blob...");
                    await blobStorageService.DeleteBlobAsync(containerName, deleteBlobName);
                    break;
                case "5":
                    Console.WriteLine("Exiting program...");
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
}
