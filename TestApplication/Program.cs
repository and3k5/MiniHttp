using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using And3k5.MiniHttp;

namespace TestApplication
{
    internal class Program
    {
        private static void Main()
        {
            using var server = new WebServer(Handler)
            {
                SimultaneouslyHandlers = 5
            };
            server.OnReceivedRequest += () => Console.WriteLine("Got a request");
            server.OnClosedResponse += () => Console.WriteLine("Closed a response");
            server.Listen(8084);
            Console.WriteLine("Press enter to close");

            for (var i = 0; i < 10; i++)
            {
                new Thread(() =>
                {
#pragma warning disable 4014
                    DoRequest();
#pragma warning restore 4014
                }).Start();
            }

            Console.ReadLine();
        }

        private static void Handler(MiniHttpContext context)
        {
            Thread.Sleep(1000);
            context.SetResponse("Hej");
            context.Response.Close();
        }

        public static async Task<HttpResponseMessage> DoRequest()
        {
            var requestUri = new Uri("http://127.0.0.1:8084/");


            using var s = new HttpRequestMessage(HttpMethod.Get, requestUri);
            using var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                MaxConnectionsPerServer = 40
            };

            using var httpClient = new HttpClient(httpClientHandler);

            var sendAsync = httpClient.SendAsync(s);
            var httpResponseMessage = await sendAsync;
            return httpResponseMessage;
        }
    }
}