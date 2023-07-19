using System.Text;

namespace Alterna
{
    public class LocalFileDownloader : IFileDownloader
    {
        public byte[] DownloadFile(string fileUrl)
        {
            byte[] demoByteArray = Encoding.UTF8.GetBytes("This is a demo file content.");
            return demoByteArray;
        }
    }
}