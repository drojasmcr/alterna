namespace Alterna
{
    public interface IFileDownloader 
    {
        byte[] DownloadFile(string fileUrl);
    }
}