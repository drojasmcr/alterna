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
        public async Task<List<ConversationHistory>> GetConversationsAsync(string startTimestamp, string endTimestamp, ILogger log)
        {
            
            //GET CONVERSATION TRANSCRIPTS
            try
            {
                string conversationTranscriptURL = "https://qa.alternasavings.ustage.app/app/rest/v3/conversationhistory/search";
                string payload = "{  \"$_type\": \"ConversationHistoryQuery\",";

                if ( !string.IsNullOrEmpty(startTimestamp) || !string.IsNullOrEmpty(endTimestamp)) // there is a start or an end time stamp parameter - or both
                {
                    payload += " \"searchFilters\": [ ";
                    payload += GetFilter(startTimestamp, "GREATER_THAN") + "},";
                    payload += GetFilter(endTimestamp, "LOWER_THAN");
                    payload +=  "}],";

                }

                payload += "  \"orderBy\": [{\"$_type\": \"ConversationHistoryOrderBy\",\"field\": \"CREATION_TIMESTAMP\",\"order\": \"ASCENDING\"}],";
                payload += " \"offset\": 0, \"limit\": 10 }";

                StringContent content = new(payload, Encoding.UTF8, "application/json");

                HttpClient client = new();
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{User}:{Password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);  

                log.LogInformation("DEBUG:URL {0}", conversationTranscriptURL);
                log.LogInformation("DEBUG: User: {0}, Password {1}", User, Password);
                log.LogInformation("DEBUG: Payload: {0}", payload);

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

        static string GetFilter(string currentTimestamp, string filterOperator)
        {
            string payload = string.Empty;
            if (!string.IsNullOrEmpty(currentTimestamp))
            {
                payload += "{ \"$_type\": \"SendTimestampMessageSearchFilter\", \"field\": \"CREATION_TIMESTAMP\",";
                payload += " \"operator\": {  \"$_type\": \"EqualsTimestampOperator\", \"type\": \"" + filterOperator + "\", \"value\": \"" + currentTimestamp + "\"}";
            }
            return payload;
        }
    }
}