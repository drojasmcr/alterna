using Newtonsoft.Json;
public class ConversartionRecording 
{
        [JsonProperty("$_type")]
        public string _type { get; set; }
        public string conversationId { get; set; }
        public string blobStoreId { get; set; }
        public string fileName { get; set; }
        public string mimeType { get; set; }
        public int totalSize { get; set; }
        public string downloadLink { get; set; }
        public long recordingStartTimestamp { get; set; }
        public long recordingEndTimestamp { get; set; }
        public string status { get; set; }
        public string endReason { get; set; }
        public string recordingType { get; set; }
}