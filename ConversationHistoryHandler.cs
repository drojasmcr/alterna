using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Alterna
{
    public class ConversationHistoryHandler
    {
        private string User { get; set; }
        private string Password { get; set; }
        public ConversationHistoryHandler(string user, string password)
        {
            User = user;
            Password = password;
        }
        public async Task<List<ConversationHistory>> GetConversationsAsync(string state, string endTimestamp, ILogger log)
        {
            
            //GET CONVERSATION TRANSCRIPTS
            try
            {
                string conversationTranscriptURL = "https://qa.alternasavings.ustage.app/app/rest/v3/conversationhistory/search";
                string payload = "{  \"$_type\": \"ConversationHistoryQuery\",";

                if ( !string.IsNullOrEmpty(endTimestamp)) // there is a end time stamp parameter
                {
                    payload += " \"searchFilters\": [{ \"$_type\": \"SendTimestampMessageSearchFilter\", \"field\": \"END_TIMESTAMP\",";
                    payload += " \"operator\": {  \"$_type\": \"EqualsTimestampOperator\", \"type\": \"LOWER_THAN\", \"value\": \"";
                    payload += endTimestamp +  "\"}}],";

                }

                payload += "  \"orderBy\": [{\"$_type\": \"ConversationHistoryOrderBy\",\"field\": \"CREATION_TIMESTAMP\",\"order\": \"ASCENDING\"}],";
                payload += " \"offset\": 0, \"limit\": 40 }";

                StringContent content = new(payload, Encoding.UTF8, "application/json");

                HttpClient client = new();
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{User}:{Password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);  

                HttpResponseMessage searchResponse = await client.PostAsync(conversationTranscriptURL, content);
                if ( searchResponse.IsSuccessStatusCode )
                {
                    string responseBody = await searchResponse.Content.ReadAsStringAsync();      
                    List<ConversationHistory> conversations = JsonSerializer.Deserialize<List<ConversationHistory>>(responseBody);    
                    return conversations;         
                }
                else 
                {
                    log.LogError("API request failed while invoking method searchMessages. StatusCode: {0}", searchResponse.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {                
                log.LogError(ex, "An error occurred while invoking the API with the method searchMessages.");
                return null;
            }
        }
    }
}