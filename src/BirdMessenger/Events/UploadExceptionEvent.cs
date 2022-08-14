using System;
using System.Net.Http;

namespace BirdMessenger.Delegates;

public class UploadExceptionEvent:UploadEvent
{
    public UploadExceptionEvent(TusRequestOptionBase tusRequestOption,Exception exception) : base(tusRequestOption)
    {
        Exception = exception;
    }
    /// <summary>
    /// origin http response
    /// </summary>
    public HttpResponseMessage OriginResponseMessage { get; set; }
    
    
    /// <summary>
    /// origin HttpRequestMessage
    /// </summary>
    public HttpRequestMessage OriginHttpRequestMessage { get; set; }
    
    /// <summary>
    /// exception arose while uploading 
    /// </summary>
    public Exception Exception { get; }
}