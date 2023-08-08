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
    public class ConversationHistoryManager
    {
        private string User { get; set; }
        private string Password { get; set; }
        public ConversationHistoryManager(string user, string password)
        {
            User = user;
            Password = password;
        }
        public async Task<List<ConversationHistory>> GetConversationsAsync(string state, DateTime startDate, ILogger log)
        {
            double startTimeStamp = -1;
            if (startDate != DateTime.MinValue) //!null value
            {
                startTimeStamp = getTimeStampFromDate(startDate);//!null value
            }


            
            //GET CONVERSATION TRANSCRIPTS
            try
            {
                string conversationTranscriptURL = "https://qa.alternasavings.ustage.app/app/rest/v3/conversationhistory/search";
                string payload = "{  \"$_type\": \"ConversationHistoryQuery\",";

                if ( startTimeStamp != -1 ) // there is a creation time stamp parameter
                {

                }

                payload += "  \"orderBy\": [{\"$_type\": \"ConversationHistoryOrderBy\",\"field\": \"CREATION_TIMESTAMP\",\"order\": \"ASCENDING\"}],";
                payload += " \"offset\": 0, \"limit\": 20 }";

                StringContent content = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpClient client = new HttpClient();
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
                    log.LogError("API request failed while invoking method searchMessages.");
                    return null;
                }
            }
            catch (Exception ex)
            {                
                log.LogError(ex, "An error occurred while invoking the API with the method searchMessages.");
                return null;
            }
        }

        private double getTimeStampFromDate(DateTime dateTime)
        {
            var timeStamp = dateTime.Subtract(DateTime.UnixEpoch).TotalSeconds;
            return timeStamp;
        }
    }
}