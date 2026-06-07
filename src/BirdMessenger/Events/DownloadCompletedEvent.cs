using System.Net.Http;

namespace BirdMessenger.Events;

public class DownloadCompletedEvent : DownloadEvent
{
    public DownloadCompletedEvent(TusRequestOptionBase tusRequestOption, HttpResponseMessage httpResp) : base(
        tusRequestOption)
    {
        OriginResponseMessage = httpResp;
    }

    public HttpResponseMessage OriginResponseMessage { get; }
}
