using System;
using System.IO;
using System.Threading.Tasks;
using BirdMessenger.Events;

namespace BirdMessenger;

public class TusDownloadRequestOption : TusRequestOptionBase
{
    public Uri FileLocation { get; set; }

    public Stream OutputStream { get; set; }

    public uint DownloadBufferSize = 1 * 1024 * 1024;

    public Func<DownloadProgressEvent, Task>? OnProgressAsync { get; set; }

    public Func<DownloadCompletedEvent, Task>? OnCompletedAsync { get; set; }

    public Func<DownloadExceptionEvent, Task>? OnFailedAsync { get; set; }
}
