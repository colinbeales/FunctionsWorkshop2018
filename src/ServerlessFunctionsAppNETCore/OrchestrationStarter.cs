using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using ServerlessFunctionsAppNETCore.Orchestrations;

namespace ServerlessFunctionsAppNETCore
{
    public class OrchestrationStarter
    {
        [FunctionName("UrlScraperOrchestration_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            var input = await req.Content.ReadAsStringAsync();

            string instanceId = await starter.StartNewAsync(nameof(UrlScraperOrchestration), input);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
