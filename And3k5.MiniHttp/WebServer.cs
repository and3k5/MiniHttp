using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace And3k5.MiniHttp
{
    public class WebServer : IDisposable
    {
        public WebServer(Action<MiniHttpContext> handler)
        {
            Handler = handler;
        }

        public int SimultaneouslyHandlers
        {
            get => _simultaneouslyHandlers;
            set
            {
                if (value < 1)
                    throw new Exception("SimultaneouslyHandlers should be at least 1");
                _simultaneouslyHandlers = value;
            }
        }

        public delegate void ClosedResponse();

        public delegate void ReceivedRequest();

        public event ClosedResponse OnClosedResponse;
        public event ReceivedRequest OnReceivedRequest;

        public Action<MiniHttpContext> Handler { get; }

        private HttpListener _listener;
        private CancellationTokenSource _source;
        private bool _disposed;
        private int _simultaneouslyHandlers = 1;

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

            var iterations = SimultaneouslyHandlers;


            for (var i = 0; i < iterations; i++)
            {
                _listener.BeginGetContext(ar => NewContext(_source.Token, ar), null);
            }
        }

        private void NewContext(CancellationToken cancellationToken, IAsyncResult asyncResult)
        {
            if (_listener == null)
                return;
            if (cancellationToken.IsCancellationRequested)
                return;


            var context = _listener.EndGetContext(asyncResult);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }


            OnReceivedRequest?.Invoke();

            Handler(new MiniHttpContext(context));

            context.Response.Close();
            OnClosedResponse?.Invoke();
            _listener.BeginGetContext(ar => NewContext(cancellationToken, ar), null);
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