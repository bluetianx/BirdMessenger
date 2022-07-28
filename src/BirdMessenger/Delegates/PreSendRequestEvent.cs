using System.Net.Http;

namespace BirdMessenger.Delegates;

public sealed class PreSendRequestEvent:UploadEvent
{
    public PreSendRequestEvent(TusRequestOptionBase reqOption, HttpRequestMessage httpRequestMsg)
    :base(reqOption)
    {
        HttpRequestMsg = httpRequestMsg;
    }
    /// <summary>
    /// HttpRequestMsg is send to server
    /// </summary>
    public HttpRequestMessage HttpRequestMsg { get;  }
}