using System;
using System.IO;

namespace BirdMessenger.Infrastructure
{
    public interface ITusUploadContext
    {
        long TotalSize { get; }

        long UploadedSize { get; }

        double UploadPercentage { get; }

        FileInfo UploadFileInfo { get; }

        Uri UploadFileUrl { get; }
    }
}
