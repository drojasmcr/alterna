using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
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

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()        
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            //Add user name and password to the header
            IConfigurationRoot configuration = configurationBuilder.Build();
            string userName = configuration["AuthorizedUser"]; 
            string password = configuration["UserPassword"];
            //Setting up Http client and variables
            HttpClient client = new();
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);     

            
            string conversationId = req.Query["ConversationId"];    
            List<ConversationHistory> conversations = new(); //Conversations will go here

            if (string.IsNullOrEmpty(conversationId)) //If true, then we will go and look for conversations with the other parameters
            {
                string state = req.Query["State"];
                string endTimestamp = req.Query["endDateTime"];

                ConversationHistoryHandler conversationHistoryHandler = new(userName, password);
                conversations = await conversationHistoryHandler.GetConversationsAsync(state, endTimestamp, log);
                if (conversations == null)
                {
                    log.LogError("No conversations found using the parameters provided");
                    return new StatusCodeResult((int) StatusCodes.Status204NoContent);
                }
            
            }
            else //ConversationId provided, just go and look for a specific conversation
            {
                conversations.Add(new ConversationHistory { id = conversationId});
            }

            //************************************
            //***Lets process each conversation***
            //************************************

            string conversationURL = configuration["ConversationURL"];
            
            foreach (var _conversation in conversations)
            {
                //*******************************
                //***GET CONVERSATION MESSAGES***
                //*******************************
            
                try
                {
                    ConversationMessageHandler conversationMessageHandler = new(log);

                    ConversationMessage conversationMessage = await conversationMessageHandler.GetConversationMessages(_conversation.id, DateTime.MinValue);
                    if ( conversationMessage != null)
                    {
                        string jsonMetadata = JsonSerializer.Serialize(conversationMessage);
                        string fileName = _conversation.id + "_ConversationHistoryMessageData.json";
                        byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonMetadata);
                        BlobUploader blobUploader = new();
                        int result = await blobUploader.UploadAsync(fileName, data);
                    }
                    else
                    {
                        log.LogError( "An error occurred while invoking the API with the method ConversationHistory/{0}/search. ConversationId: {0}."
                            , _conversation.id);
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "An error occurred while invoking the API with the method ConversationHistory/{0}/search. ", _conversation.id);
                }

                
                string newConversationURL = conversationURL + _conversation.id;
                //********************************************
                //*** GET CONVERSATION RECORDINGS - METADATA ***
                //********************************************
                var recordings = new List<ConversartionRecording>();

                try 
                {
                    HttpResponseMessage conversationDetailResponse = await client.GetAsync(newConversationURL);
                    if ( conversationDetailResponse.IsSuccessStatusCode )
                    {
                        string responseBody = await conversationDetailResponse.Content.ReadAsStringAsync();    
                        recordings = JsonSerializer.Deserialize<List<ConversartionRecording>>(responseBody);    
                    }
                    else
                    {
                        log.LogError(new Exception(), "An error occurred while invoking the API with the method getConversationRecordings. ConversationId: {0}. StatusCode: {1}"
                            , _conversation.id, conversationDetailResponse.StatusCode);
                    }
                }    
                catch ( Exception ex )        
                {
                    log.LogError(ex, "An error occurred while invoking the API with the method getConversationRecordings. ConversationId: {0}", _conversation.id);
                }

                //************************************************
                //*** END GET RECORDING INFORMATION - METADATA ***
                //************************************************

                if ( recordings.Count == 0) //***No recordings, so let's work with the next value
                {
                    log.LogError("Conversation: {0} does not have conversation recordings", _conversation.id);
                }
                else
                {                    
                    //*************************************************
                    //*** LETS UPLOAD THE FILE AND METADATA TO AZURE***
                    //*************************************************

                   
                    //PROCESS EACH FILE
                    foreach (var item in recordings)
                    {              
        
                        string fileUrl = item.downloadLink;
                        string fileName = item.fileName;

                        //Download the file
                        IFileDownloader fileDownloader = new LocalFileDownloader();                      
                        byte[] data = fileDownloader.DownloadFile(fileUrl);
                                        
                        //Upload file to Azure
            
                        log.LogInformation("Uploading conversation recording");
            
                        string blobName = _conversation.id + "_" + fileName;
                        
                        var blobUploader = new BlobUploader();
                        int result = await blobUploader.UploadAsync(blobName, data);

                        if (result == 0)
                        {
                            log.LogInformation("Conversation recording uploaded.");
                        }
                    }
                    //***************************************************
                    //*** END UPLOADING THE FILE AND METADATA TO AZURE***
                    //***************************************************        
                } //***No Conversation Recordings***        
            } //*** End processing each conversation
            
            string responseMessage = "Function executed successfully. Conversation(s)  uploaded successfully";
            return new OkObjectResult(responseMessage);
        }
    }
}
