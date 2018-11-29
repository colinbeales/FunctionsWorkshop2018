using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using ServerlessFunctionsAppNETCore.Activities;
using ServerlessFunctionsAppNETCore.Models;

namespace ServerlessFunctionsAppNETCore.Orchestrations
{
    public static class UrlScraperOrchestration
    {
        [FunctionName(nameof(UrlScraperOrchestration))]
        public static async Task<IEnumerable<ExtractedDocument>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var url = context.GetInput<string>();

            var document = await context.CallActivityAsync<ExtractedDocument>(nameof(LinkSourceExtractorActivity), url);

            var tasks = new List<Task<ExtractedDocument>>();
            foreach (var urlSource in document.ChildUrls)
            {
                var task = context.CallActivityAsync<ExtractedDocument>(nameof(LinkSourceExtractorActivity), urlSource);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            var result = tasks.Select(task => task.Result);

            return result;
        }
    }
}