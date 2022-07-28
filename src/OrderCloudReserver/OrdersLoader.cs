using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderCloudReserver
{
    public class OrdersLoader
    {
        [FunctionName("OrdersLoader")]
        public void Run([ServiceBusTrigger("orders")]string myQueueItem, ILogger log)
        {

            string connectionStringBlob = "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=karafstacc;AccountKey=tUsnd3rsEloJ34wzvwKi9QIqLHwro9syk4x3SXRbdo3sj0LTwDGRvjQ6RQZ3s2w88WbvPWrNxTG4+ASt/WoY4A==;BlobEndpoint=https://karafstacc.blob.core.windows.net/;FileEndpoint=https://karafstacc.file.core.windows.net/;QueueEndpoint=https://karafstacc.queue.core.windows.net/;TableEndpoint=https://karafstacc.table.core.windows.net/";
            string fileContainerName = "orders";

            dynamic data = JsonConvert.DeserializeObject(myQueueItem);
            string strId = data?.Id;

            if (!string.IsNullOrEmpty(strId))
            {
                var blobStorage = new BlobStorage(connectionStringBlob, fileContainerName);
                blobStorage.Initialize().GetAwaiter().GetResult();

                byte[] byteArray = Encoding.ASCII.GetBytes(myQueueItem);
                MemoryStream stream = new MemoryStream(byteArray);

                blobStorage.Save(stream, strId);
            }

            var responseMessage = $"C# ServiceBus queue trigger function processed message: {myQueueItem}";
            log.LogInformation(responseMessage);
            //return new OkObjectResult(responseMessage);
        }
    }
}
