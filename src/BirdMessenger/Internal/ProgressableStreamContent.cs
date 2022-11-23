using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BirdMessenger.Internal;

internal sealed class ProgressableStreamContent : HttpContent
{
    private readonly Stream _content;
    private readonly int _uploadBufferSize;
    private readonly long _uploadLength;
    private readonly Func<long, Task> _uploadProgress;

    public ProgressableStreamContent(Stream content, int uploadBufferSize, Func<long, Task> uploadProgress)
    {
        _content = content;
        _uploadBufferSize = uploadBufferSize;
        _uploadProgress = uploadProgress;

        _uploadLength = content.Length - content.Position;
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

                await _uploadProgress(_content.Position);
            }
        }
#else
    private async Task SerializeToStreamAsync(Stream stream,
        CancellationToken ct)
    {
        var buffer = new byte[uploadBufferSize];

        while (true)
        {
            var bytesRead = await content.ReadAsync(buffer, 0, buffer.Length, ct);

            if (bytesRead <= 0)
            {
                break;
            }

            await stream.WriteAsync(buffer, 0, bytesRead, ct);

            await uploadProgress(content.Position);
        }
    }
#endif
    protected override bool TryComputeLength(out long length)
    {
        length = _uploadLength;

        return true;
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