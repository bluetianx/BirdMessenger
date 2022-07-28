using System.Net.Http;

namespace BirdMessenger.Abstractions;

public abstract class TusResponseBase
{
    /// <summary>
    /// origin http response
    /// </summary>
    public HttpResponseMessage OriginResponseMessage { get; set; }
    
    
    /// <summary>
    /// origin HttpRequestMessage
    /// </summary>
    public HttpRequestMessage OriginHttpRequestMessage { get; set; }
}