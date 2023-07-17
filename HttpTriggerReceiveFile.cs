using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Alterna
{
    public static class HttpTriggerReceiveFile
    {
        [FunctionName("HttpTriggerReceiveFile")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            //Setting up Http client and variables
            string conversationId = req.Query["ConversationId"];    

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()        
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            IConfigurationRoot configuration = configurationBuilder.Build();
            string userName = configuration["AuthorizedUser"]; 
            string password = configuration["UserPassword"];

            string conversationURL = configuration["ConversationURL"] + conversationId;

            string connectionString = configuration["azureConnectionString"]; 
            string containerName = configuration["azureContainerName"]; 

            string transcripts = string.Empty;

            HttpClient client = new HttpClient();
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);       

            //GET CONVERSATION TRANSCRIPTS
            try
            {
                string conversationTranscriptURL = configuration["ConversationTranscriptURL"] + conversationId + "/searchMessages";
                string payload = "{ \"$_type\": \"MessageQuery\", \"searchFilters\": "; 
                payload +=  "[ { \"$_type\": \"SendTimestampMessageSearchFilter\", \"field\": \"SEND_TIMESTAMP\", \"operator\": { ";
                payload += " \"$_type\": \"EqualsTimestampOperator\", \"type\": \"LOWER_THAN\", \"value\": 1680547872155 } } ], ";
                payload += " \"orderBy\": [{ ";
                payload += "\"$_type\": \"MessageOrderBy\", \"field\": \"SEND_TIMESTAMP\",\"order\": \"ASCENDING\"";
                payload += "}], \"offset\": \"0\", \"limit\": \"1000\" }";

                StringContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage transcriptResponse = await client.PostAsync(conversationTranscriptURL, content);
                if ( transcriptResponse.IsSuccessStatusCode )
                {
                    string responseBody = await transcriptResponse.Content.ReadAsStringAsync();   
                    transcripts = responseBody;                 
                }
                else 
                {
                    log.LogError("API request failed while invoking method searchMessages.");
                    return new StatusCodeResult((int) transcriptResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {                
                log.LogError(ex, "An error occurred while invoking the API with the method searchMessages.");
                return new StatusCodeResult(500);
            }

            //GET RECORDING INFORMATION
            var recordings = new List<ConversartionRecording>();
            
            
            try 
            {
                HttpResponseMessage conversationDetailResponse = await client.GetAsync(conversationURL);
                if ( conversationDetailResponse.IsSuccessStatusCode )
                {
                    string responseBody = await conversationDetailResponse.Content.ReadAsStringAsync();    
                    using ( JsonDocument document = JsonDocument.Parse(responseBody) )            
                    {
                        if (document.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            
                            foreach (JsonElement element  in document.RootElement.EnumerateArray())
                            {
                                var recording = new ConversartionRecording();

                                recording.FileName = element.GetProperty("fileName").GetString();
                                recording.BlobStoreId = element.GetProperty("blobStoreId").GetString();
                                recording.MimeType = element.GetProperty("mimeType").GetString();
                                recording.TotalSize = element.GetProperty("totalSize").GetInt32();
                                recording.DownloadLink = element.GetProperty("downloadLink").GetString();
                                recording.RecordingStartTimestamp = element.GetProperty("recordingStartTimestamp").GetInt64();
                                recording.RecordingEndTimestamp = element.GetProperty("recordingEndTimestamp").GetInt64();
                                recording.Status = element.GetProperty("status").GetString();
                                recording.EndReason = element.GetProperty("endReason").GetString();
                                recording.RecordingType = element.GetProperty("recordingType").GetString();

                                recordings.Add(recording);
                            }
                        }
                    }
                }
                else
                {
                    string errorMessage = "Error getting the conversation information.";
                    return new OkObjectResult(errorMessage);
                }
            }    
            catch ( Exception ex )        
            {
                log.LogError(ex, "An error occurred while invoking the API with the method getConversationRecordings.");
                return new StatusCodeResult(500);
            }

            //PROCESS EACH FILE
            foreach (var item in recordings)
            {              
  
              string fileUrl = item.DownloadLink;
              string fileName = item.FileName;

              IFileDownloader fileDownloader = new LocalFileDownloader();                      
              byte[] data = fileDownloader.DownloadFile(fileUrl);
              
              try
              {
                  //Download the file
                  HttpResponseMessage response = await client.GetAsync(fileUrl);
                  if ( response.IsSuccessStatusCode )
                  {
                      using ( MemoryStream memoryStream = new MemoryStream())
                      {
                          await response.Content.CopyToAsync(memoryStream);
                          var tmpData = memoryStream.ToArray();
                          data = new byte[tmpData.Length];
                          tmpData.CopyTo(data, 0);
                      }
  
                      log.LogInformation("File downloaded and saved as {filePath}");
                  }
                  else 
                  {
                      string errorMessage = "Error downloading the file.";
                      return new OkObjectResult(errorMessage);
                  }
              }
              catch ( Exception ex)
              {
                  log.LogError(ex, "An error occurred while downloading the file.");
                  return new StatusCodeResult(500);
              }
  
              //Upload file to Azure
  
              log.LogInformation("C# HTTP trigger function processed a request.");
                
              
              string blobName = fileName;
  
              CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
              CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
              CloudBlobContainer container = blobClient.GetContainerReference(containerName);
              
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
            }
            

            string responseMessage = "Function executed successfully. Conversation(s)  uploaded successfully";

            return new OkObjectResult(responseMessage);
        }
    }
}
