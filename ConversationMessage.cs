using Newtonsoft.Json;
using System.Collections.Generic;

namespace Alterna
{
    public class Item
    {
        [JsonProperty("$_type")]
        public string _type { get; set; }
        public string id { get; set; }
        public string conversationId { get; set; }
        public object sendTimestamp { get; set; }
        public object serverTimestamp { get; set; }
        public string senderPersonId { get; set; }
        public string type { get; set; }
        public bool @internal { get; set; }
        public List<object> recipientPersonIds { get; set; }
        public string botThreadId { get; set; }
        public object rejectionSeverity { get; set; }
        public object rejectionReason { get; set; }
        public string text { get; set; }
    }

    public class ConversationMessage
    {
        [JsonProperty("$_type")]
        public string _type { get; set; }
        public bool hasMoreItems { get; set; }
        public object nextOffset { get; set; }
        public List<Item> items { get; set; }
    }

}