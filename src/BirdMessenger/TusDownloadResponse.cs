using BirdMessenger.Abstractions;

namespace BirdMessenger;

public class TusDownloadResponse : TusResponseBase
{
    public long DownloadedSize { get; set; }

    public long TotalSize { get; set; }
}
