using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

namespace OrderCloudReserver
{
    public class OrdersLoader
    {
        [FunctionName("OrdersLoader")]
        public async Task Run([ServiceBusTrigger("orders")]string myQueueItem, ILogger log)
        {
            dynamic data = JsonConvert.DeserializeObject(myQueueItem);
            string strId = data?.Id;

            var policy = Policy
                .Handle<Exception>()
                .RetryAsync(2);

            if (!string.IsNullOrEmpty(strId))
            {
                try 
                {
                    await policy.ExecuteAsync(() => Save2Blob(myQueueItem, strId));
                }
                catch
                {
                    await Send2Fail(myQueueItem);
                }
            }
            var responseMessage = $"C# ServiceBus queue trigger function processed message: {myQueueItem}";
            log.LogInformation(responseMessage);
        }

        private async Task Save2Blob(string myQueueItem, string strId)
        {
            var config = new ConfigurationBuilder()
           .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
           .AddEnvironmentVariables()
           .Build();

            string connectionStringBlob = config["ConnectionStringBlob"];

            string fileContainerName = "orders";

            var blobStorage = new BlobStorage(connectionStringBlob, fileContainerName);
            blobStorage.Initialize().GetAwaiter().GetResult();

            byte[] byteArray = Encoding.ASCII.GetBytes(myQueueItem);
            MemoryStream stream = new MemoryStream(byteArray);

            await blobStorage.Save(stream, strId);
        }

        private async Task Send2Fail(string myQueueItem)
        {
            const string TopicName = "fails";

            var config = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            string serviceBusConnectionString = config["AzureWebJobsServiceBus"];

            // By leveraging "await using", the DisposeAsync method will be called automatically once the client variable goes out of scope.
            // In more realistic scenarios, you would store off a class reference to the client (rather than to a local variable) so that it can be used throughout your program.
            await using var client = new ServiceBusClient(serviceBusConnectionString);

            await using ServiceBusSender sender = client.CreateSender(TopicName);

            try
            {
                var message = new ServiceBusMessage(myQueueItem);
                await sender.SendMessageAsync(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }
    }
}
