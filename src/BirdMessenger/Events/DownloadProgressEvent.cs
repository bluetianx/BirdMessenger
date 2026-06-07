namespace BirdMessenger.Events;

public sealed class DownloadProgressEvent : DownloadEvent
{
    public DownloadProgressEvent(TusRequestOptionBase tusRequestOption, long? totalSize) : base(tusRequestOption)
    {
        TotalSize = totalSize;
    }

    public long? TotalSize { get; }

    public long DownloadedSize { get; set; }
}
