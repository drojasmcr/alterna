

using System.IO;
using System.Net.Http;

namespace Alterna
{
    public class HttpFileDownloader : IFileDownloader
    {
        public byte[] DownloadFile(string fileUrl)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(fileUrl);
            if ( response.IsSuccessStatusCode )
            {
                using ( MemoryStream memoryStream = new MemoryStream())
                {
                    await response.Content.CopyToAsync(memoryStream);
                    var tmpData = memoryStream.ToArray();
                    data = new byte[tmpData.Length];
                    tmpData.CopyTo(data, 0);
                }

                log.LogInformation("File downloaded and saved as {filePath}");
            }
            else 
            {
                string errorMessage = "Error downloading the file.";
                return new OkObjectResult(errorMessage);
            }
        }
    }
}

/*
//Download the file
                  HttpResponseMessage response = await client.GetAsync(fileUrl);
                  if ( response.IsSuccessStatusCode )
                  {
                      using ( MemoryStream memoryStream = new MemoryStream())
                      {
                          await response.Content.CopyToAsync(memoryStream);
                          var tmpData = memoryStream.ToArray();
                          data = new byte[tmpData.Length];
                          tmpData.CopyTo(data, 0);
                      }
  
                      log.LogInformation("File downloaded and saved as {filePath}");
                  }
                  else 
                  {
                      string errorMessage = "Error downloading the file.";
                      return new OkObjectResult(errorMessage);
                  }
*/