using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace BirdMessenger.Infrastructure
{
    public  class TusException : Exception
    {
        public TusException(string message,HttpRequestMessage originHttpRequest =default,HttpResponseMessage originHttpResponse=default) : base(message)
        {
            OriginHttpRequest = originHttpRequest;
            OriginHttpResponse = originHttpResponse;
        }
        
        public HttpRequestMessage OriginHttpRequest { get; private set; }
        
        public HttpResponseMessage OriginHttpResponse { get; private set; }
    }
}