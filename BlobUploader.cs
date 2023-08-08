using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.IO;

namespace Alterna
{
    public class BlobUploader
    {
        public string AzureConnectionString { get; set; }
        public string AzureContainerName { get; set; }
        public BlobUploader()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()        
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfigurationRoot configuration = configurationBuilder.Build();
            AzureConnectionString = configuration["azureConnectionString"]; 
            string containerName = configuration["azureContainerName"]; 
           
        }
        
        public async Task<int> UploadAsync(string blobName, byte[] data)
        {

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(AzureConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(AzureContainerName);

            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            
            await container.CreateIfNotExistsAsync();

            BlobContainerPermissions containerPermissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };

            await container.SetPermissionsAsync(containerPermissions);

            using ( MemoryStream  stream = new MemoryStream(data) )
            {
                await blob.UploadFromStreamAsync(stream);
            }

            return 0;
        }
    }
}