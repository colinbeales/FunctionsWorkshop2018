using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ServerlessFunctionsAppNETCore.Models;

namespace ServerlessFunctionsAppNETCore.Activities
{
    public class LinkSourceExtractorActivity
    {
        private static readonly HtmlWeb Web = new HtmlWeb(); 

        [FunctionName(nameof(LinkSourceExtractorActivity))]
        public static async Task<ExtractedDocument> Run(
            [ActivityTrigger] string url, 
            ILogger log)
        {
            var result = new ExtractedDocument(url);

            try
            {
                var doc = await Web.LoadFromWebAsync(url, Encoding.UTF8);
                var anchors = doc.DocumentNode.SelectNodes("//a[@href]");
                var sources = anchors
                    .Select(a => a.GetAttributeValue("href", string.Empty))
                    .Where(a => a.StartsWith("http"));

                result.ChildUrls = sources.ToList();
            }
            catch (Exception e)
            {
                log.LogError(e, $"Exception while processing {url}.");
            }

            return result;
        }
    }
}
