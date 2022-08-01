using BirdMessenger.Abstractions;

namespace BirdMessenger;

public class TusPatchResponse:TusResponseBase
{
    /// <summary>
    /// indicate the size of uploaded bytes
    /// </summary>
    public long UploadedSize { get; set; }
}