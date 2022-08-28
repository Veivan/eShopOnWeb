using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderSaverFunc.Models;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace OrderSaverFunc
{
    public static class OrderSaver
    {
        private static readonly string _endpointUri = "https://karafcosmos.documents.azure.com:443/";
        private static readonly string _primaryKey = "4FOVVtjhxkPFqBMiVz22MhWu11zRXM9m8MVzqdNm11cEJqgtdj9xHY8oIBD6u0LooUTk7trCEC8vFedWTpW3Ww==";
        private static readonly string databaseName = "OrdersDB";
        private static readonly string collectionName = "FilledOrders";

        [FunctionName("OrderSaver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string responseMessage = $"Wrong input data : {requestBody}";
            try
            {
                Order data = JsonConvert.DeserializeObject<Order>(requestBody);
                if (data != null)
                {
                    data.FinalPrice = data.Total();
                    await CreateUserDocumentIfNotExists(databaseName, collectionName, data, log);
                    responseMessage = $"Order ID = {data.Id}. This HTTP triggered function executed successfully.";
                }
            }
            catch (Exception ex)
            {
                responseMessage = ex.Message;
            }

            return new OkObjectResult(responseMessage);
        }

        private static async Task CreateUserDocumentIfNotExists(string databaseName, string collectionName, Order order, ILogger log)
        {
            using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
            {
                Container container = null;
                try
                {
                    container = client.GetContainer(databaseName, collectionName);

                    // Read the item to see if it exists
                    ItemResponse<Order> response = await container.ReadItemAsync<Order>(order.Id, new PartitionKey(order.Id));
                    log.LogInformation($"Exists - {order.Id}.");
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    ItemResponse<Order> response = await container.CreateItemAsync<Order>(order, new PartitionKey(order.Id));
                    log.LogInformation($"New - {order.Id}.");
                }

            }
        }
    }
}
