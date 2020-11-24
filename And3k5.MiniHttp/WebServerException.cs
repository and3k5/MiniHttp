using System;

// ReSharper disable UnusedMember.Global

namespace And3k5.MiniHttp
{
    public abstract class WebServerException : Exception
    {
        internal WebServerException()
        {
        }

        internal WebServerException(string message) : base(message)
        {
        }

        internal WebServerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}