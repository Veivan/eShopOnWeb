using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging.Abstractions;
using OrderItemsReserver;
using OrderSaverFunc;
using System.IO;
using System.Text;

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

        [TestMethod]
        public void TestOrderSaverSuccess()
        {
            var logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");

            var queryStringValue = "{Id:7}";
            /*var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Query = new QueryCollection
                (
                    new System.Collections.Generic.Dictionary<string, StringValues>()
                    {
                        { "name", queryStringValue }
                    }
                )
            };*/

            // Create a default HttpContext
            var httpContext = new DefaultHttpContext();

            // Create the stream to house our content
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(queryStringValue));
            httpContext.Request.Body = stream;
            httpContext.Request.ContentLength = stream.Length;

            var response = OrderSaver.Run(httpContext.Request, logger);
            response.Wait();

            // Check that the response is an "OK" response
            Assert.IsNotNull(response.Result);

        }
    }
}
