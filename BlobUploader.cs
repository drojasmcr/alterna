using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.IO;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Alterna
{
    public class BlobUploader
    {
        protected string AzureConnectionString { get; set; }
        protected string AzureContainerName { get; set; }
        public BlobUploader()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()        
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfigurationRoot configuration = configurationBuilder.Build();
            AzureConnectionString = configuration["azureConnectionString"]; 
            AzureContainerName = configuration["azureContainerName"]; 
           
        }
        
        public async Task<int> UploadAsync(string blobName, byte[] data, ConversationHistory conversationHistory, ILogger logger)
        {
            var blobServiceClient = new BlobServiceClient(AzureConnectionString);
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureContainerName);
            BlockBlobClient blockBlobClient = blobContainerClient.GetBlockBlobClient(blobName);

            using MemoryStream stream = new(data);
            await blockBlobClient.UploadAsync(stream);

            int result = await SetTags(blockBlobClient, conversationHistory, logger);

            return 0;
        }

        public static async Task<int> SetTags(BlockBlobClient blobClient, ConversationHistory conversationHistory, ILogger logger)
        {
            try 
            {
                conversationHistory.state ??= "NO STATE";
                conversationHistory.createdTimestamp ??= 0;
                conversationHistory.endTimestamp ??= 0;
                conversationHistory.initialEngagementType ??= "NO ENGAGEMENT TYPE";
                conversationHistory.recipient ??= new Recipient { displayName = "NO DISPLAY NAME"};

                Dictionary<string, string> tags = 
                    new Dictionary<string, string>
                {
                    { "createdTimestamp", conversationHistory.createdTimestamp.ToString()},
                    { "endTimestamp", conversationHistory.endTimestamp.ToString() },
                    { "State", conversationHistory.state },
                    { "initialEngagementType", conversationHistory.initialEngagementType },
                    { "displayName", conversationHistory.recipient.displayName },
                    { "ConversationHistoryid", conversationHistory.id }
                };

                await blobClient.SetMetadataAsync(tags);

                await blobClient.SetTagsAsync(tags);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding metadata. Conversation Id {0}, DiplayName: {1}", conversationHistory.id, conversationHistory.recipient.displayName);
            }

            return 0;
        }
    }
}