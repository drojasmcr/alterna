using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Alterna
{
    public class ConversationMessageHandler
    {
        protected string User { get; set; }
        protected string Password {get; set;}
        protected string ConversationMessages { get; set; }
        
        protected IConfigurationRoot Configuration { get; set; }

        protected ILogger Logger {get;set;}
        public ConversationMessageHandler(ILogger log)
        {

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()        
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
                        Configuration  = configurationBuilder.Build();
            User = Configuration["AuthorizedUser"]; 
            Password = Configuration["UserPassword"];
            ConversationMessages = Configuration["ConversationMessages"];
            Logger = log;
        }

        public async Task<ConversationMessage> GetConversationMessages(string conversationId)
        {
                

                string payload = "{    \"$_type\": \"MessageQuery\", \"orderBy\": [{\"$_type\": \"MessageOrderBy\",\"field\": \"SEND_TIMESTAMP\",\"order\": \"ASCENDING\"}],";
                payload += " \"offset\": 0, \"limit\": 1000 }";

                ConversationMessages = ConversationMessages + conversationId + "/searchMessages";

                StringContent content = new(payload, Encoding.UTF8, "application/json");

                HttpClient client = new();
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{User}:{Password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);  

                HttpResponseMessage conversationMessagesResponse = await client.PostAsync(ConversationMessages, content);
                if ( conversationMessagesResponse.IsSuccessStatusCode )
                {
                    string responseBody = await conversationMessagesResponse.Content.ReadAsStringAsync();      
                    ConversationMessage conversationMessages = JsonSerializer.Deserialize<ConversationMessage>(responseBody);    
                    return conversationMessages;         
                }
                else 
                {
                    Logger.LogError("An error occurred while invoking the API with the method ConversationHistory/{0}/search. ConversationId: {0}. StatusCode: {1}"
                            , conversationId, conversationMessagesResponse.StatusCode);
                    return null;
                }
        }

    }
}