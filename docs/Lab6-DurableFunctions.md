# Lab 6 - Durable Functions

In this lab you will learn how to program durable functions using the Durable Extensions Framework.

Goals for this lab: 
- [A durable Hello World function](#1)
- [Building an orchestration and activities](#2)
- [Fan-out/fan-in](#3)

## <a name="1"></a>1. A durable Hello World function

The first thing you will do to get acquianted with Durable Functions is to create and execute a Hello World version.

Add a new function to the project and this time select the ```Durable Functions Orchecstration``` template.

This should add a single file to the project. Spend some time examining the contents of that file.

> How many functions do you see inside the single file?
> What is the purpose of each of the functions in the orchestration?

Run your project and you should see the new functions appear. Copy and paste the URL of the new function with _HttpStart in the name. The function will start the orchestration and return JSON data to your browser. It contains information about the instance of the orchestration you have just started, including some URLs to check the status. Copy and paste ```statusQueryGetUri``` to a new browser tab and check the status. 

## <a name="2"></a>2. Building an orchestration and activities

Next you will build an orchestration that will find link sources in a web page and stores the information as json in blob storage. 

Begin by adding three folders to your project: 
1. Activities
2. Orchestrations
3. Models

Add a model class ```ExtractedDocument``` to the models folder. This model will contain two properties to capture the parent and child urls.
```
public class ExtractedDocument
{
    public ExtractedDocument(string parentUrl)
    {
        ParentUrl = parentUrl;
        ChildUrls = new List<string>();
    }
    public string ParentUrl { get; set; }
    public List<string> ChildUrls { get; set; }
}
```

Add a new class to the Activities folder and name it ```LinkSourceExtractorActivity```. Implement it with the following code, which is common from the queue triggered LinkSourceExtractor function mostly:
```
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

```
> Notice that the creation of HtmlWeb is done in a static field outside function. Why could this be useful?

Then add a new class to the Orchestrations folder, and name it ```UrlScraperOrchestration```.
Implement it with the following code:
```
[FunctionName(nameof(UrlScraperOrchestration))]
public static async Task<ExtractedDocument> RunOrchestrator(
    [OrchestrationTrigger] DurableOrchestrationContext context)
{
    var url = context.GetInput<string>();
    var document = await context.CallActivityAsync<ExtractedDocument>(nameof(LinkSourceExtractorActivity), url);
    
    return document;
}
```

It doesn't look very spectacular yet since there is just one activity called in this orchestration, but that will change in the next section.

Finally, add a Function that triggers the start of the orchestration at the root of the project.
```
[FunctionName("UrlScraperOrchestration_HttpStart")]
public static async Task<HttpResponseMessage> HttpStart(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage req,
    [OrchestrationClient]DurableOrchestrationClient starter,
    ILogger log)
{
    var input  = await req.Content.ReadAsStringAsync();
    string instanceId = await starter.StartNewAsync(nameof(UrlScraperOrchestration), input);
    log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
    return starter.CreateCheckStatusResponse(req, instanceId);
}
```

Compile the project, fix any errors and start the project. Use Postman of the VSCode REST client to do a POST to the HTTP start function and provide a valid url in the body (e.g. http://docs.microsoft.com). 

> Put some breakpoints in the orchestrator and the activity function and start the orchestration.
>
> How many times is a breakpoint hit in the orchestration? 
>
> How many times in the activity function?
> 
> Open the Azure Storage Explorer and look for a table called DurableFunctionsHubHistory. What can you see in there?


As you might have noticed the orchestration function is being replayed. The Durable Functions framework stored the orchestration status in Azure Table Storage and uses Storage Queues to call activities and start/replay the orchestration.   

## <a name="3"></a>3. Fan-out/fan-in

Let's make the orchestration a bit more interesting. Once the LinkSourceExtractorActivity has been called for the given url an ExtractedDocument is returned which contains a list of urls in the ChildUrls property. In the orchestration we can iterate over this list and call the LinkSourceExtractorActivity function for each of these child url items. Once all of those items have been processed we can combine the results and return this aggregate answer to the user. This pattern is known as fan-out/fan-in. 

Change the implementation of the orchestration function to the following:
```
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
```
> Notice that the calls to the activity function in the foreach loop are not awaited. There's only one await for the entire collection of tasks.

Be careful which site you select to start with since links will be retrieved for two levels deep! You can put a breakpoint in the orchestration right after the first call to LinkSourceExtractorActivity to see how big the ChildUrls list is.

Now compile and start the orchestration (again with Postman or VSCode REST Client). Keep an eye on the function runtime console to see how often activity functions are started. Once the orchestration is finished use the ```statusQueryGetUri``` to retrieve the final results.

## <a name="4"></a>4. If you have time left...

On the next part, you are on your own. Here are the tasks you need to complete:
1. Refactor the HTTP starter function to be more generic and able to start any other orchestration within the Function App.
2. Have a look at the various methods in the DurableOrchestrationContext(Base) class. There are many ways to call an Activity function.
3. Write unit tests for the orchestration and activity function.

## Wrapup
In this lab you have created your first durable functions and learned how to partition these into orchestrations and activities. You've used composition patterns such as chaining and fan-out/fan-in.

With this, you have successfully completed your labs for this workshop. Feel free to experiment with Azure Functions and discover and learn more about this amazing framework and programming model.
