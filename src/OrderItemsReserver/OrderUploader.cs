using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderItemsReserver
{
    public static class OrderUploader
    {
        [FunctionName("OrderUploader")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string connectionString = "connectionString";
            string fileContainerName = "files";

            log.LogInformation("C# HTTP trigger function processed a request.");

            string access_token = null; // req.Query["access_token"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string name = access_token ?? data?.Id;

            if (!string.IsNullOrEmpty(name))
            {
                var blobStorage = new BlobStorage(connectionString, fileContainerName);
                blobStorage.Initialize().GetAwaiter().GetResult();
                await blobStorage.Save(req.Body, name);
            } 

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Order# {name} saved. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
