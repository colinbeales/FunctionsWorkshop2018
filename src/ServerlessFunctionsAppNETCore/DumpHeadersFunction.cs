using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ServerlessFunctionsAppNETCore
{
    public static class DumpHeadersFunction
    {
        [FunctionName(nameof(DumpHeadersFunction))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var builder = new StringBuilder();
            foreach (var header in req.Headers)
            {
                builder.AppendFormat("{0}='{1}',", header.Key, string.Concat(header.Value));
            }

            return new OkObjectResult(builder.ToString());
        }
    }
}
