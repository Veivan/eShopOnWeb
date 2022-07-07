using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging.Abstractions;
using OrderItemsReserver;

namespace AzurFuncTest
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestOrderUploaderSuccess()
		{
            var logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");

            var queryStringValue = "abc";
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Query = new QueryCollection
                (
                    new System.Collections.Generic.Dictionary<string, StringValues>()
                    {
                        { "name", queryStringValue }
                    }
                )
            };

            var response = OrderUploader.Run(request, logger);
            response.Wait();

            // Check that the response is an "OK" response
            Assert.IsNotNull(response.Result);

        }
    }
}
