public class ConversartionRecording 
{
    public string FileName {get;set;}
    public string BlobStoreId {get;set;}
    public string MimeType { get;set;}
    public int TotalSize {get;set;}
    public string DownloadLink {get;set;}
    public long RecordingStartTimestamp {get;set;}
    public long RecordingEndTimestamp { get; set;}
    public string Status { get;set;}
    public string EndReason { get;set;}
    public string RecordingType {get;set;}
}