using System.Net.Http;

namespace BirdMessenger.Delegates;

public class UploadCompletedEvent:UploadEvent
{
    public UploadCompletedEvent(TusRequestOptionBase tusRequestOption,HttpResponseMessage httpResp) : base(tusRequestOption)
    {
        OriginResponseMessage = httpResp;
    }
    
    /// <summary>
    /// OriginResponseMessage from server
    /// </summary>
    public HttpResponseMessage OriginResponseMessage { get; }
}