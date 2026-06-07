using System;
using System.Net.Http;

namespace BirdMessenger.Events;

public class DownloadExceptionEvent : DownloadEvent
{
    public DownloadExceptionEvent(TusRequestOptionBase tusRequestOption, Exception exception) : base(tusRequestOption)
    {
        Exception = exception;
    }

    public HttpResponseMessage OriginResponseMessage { get; set; }

    public HttpRequestMessage OriginHttpRequestMessage { get; set; }

    public Exception Exception { get; }
}
