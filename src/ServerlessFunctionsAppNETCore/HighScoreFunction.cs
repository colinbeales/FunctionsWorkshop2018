using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ServerlessFunctionsAppNETCore
{
    public static class HighScoreFunction
    {
        [FunctionName(nameof(HighScoreFunction))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", 
            Route = "HighScores/player/{nickname}")]HttpRequest req,
            string nickname, // from route parameter in request URL
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Fetching score from body
            string score = await new StreamReader(req.Body).ReadToEndAsync();

            return int.TryParse(score, out int points) && !string.IsNullOrWhiteSpace(nickname) 
                ? (ActionResult)new OkObjectResult($"{nickname} achieved a score of {points}!") 
                : new BadRequestObjectResult($"Received invalid nickname and/or score!");
        }
    }
}
