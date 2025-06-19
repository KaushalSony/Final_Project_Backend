using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Final_Project_WebAPI.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly string _accountName;
        private readonly string _accountKey;

        public BlobStorageService(IConfiguration configuration)
        {
            _containerName = configuration.GetSection("AzureBlobStorage:ContainerName").Value!;
            _accountName = configuration.GetSection("AzureBlobStorage:AccountName").Value!;
            _accountKey = configuration.GetSection("AzureBlobStorage:AccountKey").Value!;

            var connectionString = configuration.GetSection("AzureBlobStorage:ConnectionString").Value;
            _blobServiceClient = new BlobServiceClient(connectionString);

            CreateContainerIfNotExists();
        }

        private void CreateContainerIfNotExists()
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                containerClient.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating blob container: {ex.Message}");
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(fileName);
                await blobClient.UploadAsync(fileStream, overwrite: true);

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerName,
                    BlobName = fileName,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ExpiresOn = DateTimeOffset.UtcNow.AddYears(1)
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var storageSharedKeyCredential = new Azure.Storage.StorageSharedKeyCredential(
                    _accountName,
                    _accountKey
                );

                var sasToken = sasBuilder.ToSasQueryParameters(storageSharedKeyCredential).ToString();
                return $"{blobClient.Uri}?{sasToken}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UploadFileAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
