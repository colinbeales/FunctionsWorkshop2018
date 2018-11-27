using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ServerlessFunctionsAppNETCore
{
    public static class LinkSourceExtractor
    {
        [FunctionName(nameof(LinkSourceExtractor))]
        public static async Task Run([QueueTrigger("azurefunctions-extractor-requests",
                Connection = "azurefunctions-queues")]string websiteUri,
            int dequeueCount,
            [Blob("azurefunctions-extractor-results/{rand-guid}.txt", FileAccess.ReadWrite, Connection = "azurefunctions-blobs")] CloudBlockBlob blob,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {websiteUri}");

            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(websiteUri, Encoding.UTF8);
            var anchors = doc.DocumentNode.SelectNodes("//a[@href]");
            var references = anchors.Select(a => a.GetAttributeValue("href", string.Empty));
            var content = string.Join(Environment.NewLine, references);

            using (var stream = await blob.OpenWriteAsync())
            {
                await stream.WriteAsync(Encoding.UTF8.GetBytes(content));
            }
        }
    }
}
