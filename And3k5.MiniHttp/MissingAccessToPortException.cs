using System;

// ReSharper disable UnusedMember.Global

namespace And3k5.MiniHttp
{
    public class MissingAccessToPortException : WebServerException
    {
        public MissingAccessToPortException(string prefix, int port, Exception innerException) : base(
            CreateMessage(prefix, port), innerException)
        {
        }

        public MissingAccessToPortException(string prefix, int port) : base(CreateMessage(prefix, port))
        {
            Prefix = prefix;
            Port = port;
        }

        public string Prefix { get; }
        public int Port { get; }

        private static string CreateMessage(string prefix, int port)
        {
            return "Could not start listener on port " + port + " by prefix " + prefix +
                   "\nYou can either run this application with admin rights or allow the port to be bound by running this command:" +
                   "\nnetsh http add urlacl url=" + prefix + " user=Everyone listen=yes";
        }
    }
}