using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace And3k5.MiniHttp
{
    public class WebServer : IDisposable
    {
        public WebServer(Action<MiniHttpContext> handler)
        {
            Handler = handler;
        }

        public Action<MiniHttpContext> Handler { get; }

        private HttpListener _listener;
        private Thread _thread;
        private CancellationTokenSource _source;
        private bool _disposed;

        public void Listen(int port, char hostChar = '+')
        {
            if (_listener != null)
                throw new NotImplementedException("Cannot listen to multiple ports yet");

            if (!HttpListener.IsSupported)
                throw new InvalidOperationException("HttpListener is not supported by the current operating system");

            _source = new CancellationTokenSource();

            _listener = new HttpListener();
            var uriPrefix = $"http://{hostChar}:{port}/";
            _listener.Prefixes.Add(uriPrefix);
            try
            {
                _listener.Start();
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 5 &&
                                                   RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new MissingAccessToPortException(uriPrefix, port, ex);
            }

            _thread = new Thread(() =>
            {
                var _ = ListenerLoop(_source.Token);
            });
            _thread.Start();
        }

        private async Task ListenerLoop(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (_listener == null)
                    return;
                if (cancellationToken.IsCancellationRequested)
                    return;

                var contextTask = _listener.GetContextAsync();
                while (!contextTask.Wait(100))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }

                // Note: The GetContext method blocks while waiting for a request.
                var context = await contextTask.ConfigureAwait(false);

                Handler(new MiniHttpContext(context));

                context.Response.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // TODO ??
            }

            _source?.Cancel();
            _listener?.Stop();
            _listener = null;

            _disposed = true;
        }
    }
}