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
        
        /// <summary>
        ///  origin http request
        /// </summary>
        public HttpRequestMessage OriginHttpRequest { get; private set; }
        
        /// <summary>
        ///  origin http response
        /// </summary>
        public HttpResponseMessage OriginHttpResponse { get; private set; }
    }
}