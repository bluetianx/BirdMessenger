using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BirdMessenger.Internal;

internal sealed class ProgressableStreamContentDeferLength : HttpContent
{
    private readonly Stream _content;
    private readonly uint _uploadBufferSize;
    private readonly long? _uploadLength;
    private readonly Func<long, Task> _uploadProgress;
    private long _uploadedBytes;

    public ProgressableStreamContentDeferLength(Stream content, uint uploadBufferSize, Func<long, Task> uploadProgress)
    {
        _content = content;
        _uploadBufferSize = uploadBufferSize;
        _uploadProgress = uploadProgress;

        // Try to get the length, but handle streams where Length is not available
        try
        {
            if (content.CanSeek)
            {
                _uploadLength = content.Length - content.Position;
            }
        }
        catch
        {
            // Stream doesn't support length (e.g., network stream, pipe, etc.)
            _uploadLength = null;
        }
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        return SerializeToStreamAsync(stream, default);
    }

#if NET5_0_OR_GREATER
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context,
        CancellationToken cancellationToken)
    {
        return SerializeToStreamAsync(stream, cancellationToken);
    }

    private async Task SerializeToStreamAsync(Stream stream,
        CancellationToken ct)
    {
        var buffer = new byte[_uploadBufferSize].AsMemory();

        while (true)
        {
            var bytesRead = await _content.ReadAsync(buffer, ct);

            if (bytesRead <= 0)
            {
                break;
            }

            await stream.WriteAsync(buffer[..bytesRead], ct);

            _uploadedBytes += bytesRead;

            await _uploadProgress(_uploadedBytes);
        }
    }
#else
    private async Task SerializeToStreamAsync(Stream stream,
        CancellationToken ct)
    {
        var buffer = new byte[_uploadBufferSize];

        while (true)
        {
            var bytesRead = await _content.ReadAsync(buffer, 0, buffer.Length, ct);

            if (bytesRead <= 0)
            {
                break;
            }

            await stream.WriteAsync(buffer, 0, bytesRead, ct);

            _uploadedBytes += bytesRead;

            await _uploadProgress(_uploadedBytes);
        }
    }
#endif
    protected override bool TryComputeLength(out long length)
    {
        if (_uploadLength.HasValue)
        {
            length = _uploadLength.Value;
            return true;
        }

        length = 0;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _content.Dispose();
        }

        base.Dispose(disposing);
    }
}