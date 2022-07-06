using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Threading.Tasks;

namespace OrderItemsReserver;
internal class BlobStorage
{
    private string connectionString;
    private string fileContainerName;

    public BlobStorage(string _ConnectionString, string _FileContainerName)
    {
        connectionString = _ConnectionString;
        fileContainerName = _FileContainerName;
    }


    public Task Initialize()
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(fileContainerName);
        return containerClient.CreateIfNotExistsAsync();
    }

    public Task Save(Stream data, string name)
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

        // Get the container (folder) the file will be saved in
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(fileContainerName);

        // Get the Blob Client used to interact with (including create) the blob
        BlobClient blobClient = containerClient.GetBlobClient(name);

        // Upload the blob
        return blobClient.UploadAsync(data);
    }

}
