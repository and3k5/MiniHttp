using System;
using And3k5.MiniHttp;

namespace TestApplication
{
    internal class Program
    {
        private static void Main()
        {
            using var server = new WebServer(Handler);
            server.Listen(8084);
            Console.WriteLine("Press enter to close");
            Console.ReadLine();
        }

        private static void Handler(MiniHttpContext context)
        {
            context.SetResponse("Hej");
            context.Response.Close();
        }
    }
}