using System.Net;

// ReSharper disable UnusedMember.Global

namespace And3k5.MiniHttp
{
    public class MiniHttpContext
    {
        public MiniHttpContext(HttpListenerContext context)
        {
            Context = context;
        }

        public HttpListenerContext Context { get; }
        public HttpListenerRequest Request => Context.Request;
        public HttpListenerResponse Response => Context.Response;

        public void SetResponse(string responseString)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            Response.ContentLength64 = buffer.Length;
            using (var output = Response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
                output.Flush();
            }
        }
    }
}