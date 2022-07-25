using System.Net.Http;

namespace BirdMessenger.Delegates;

public sealed class PreSendRequestEvent
{
    /// <summary>
    /// HttpRequestMsg is send to server
    /// </summary>
    public HttpRequestMessage HttpRequestMsg { get; set; }
}