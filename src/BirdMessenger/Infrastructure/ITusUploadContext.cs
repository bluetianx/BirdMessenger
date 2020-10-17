using System;
using System.IO;

namespace BirdMessenger.Infrastructure
{
    public interface ITusUploadContext
    {
        long TotalSize { get; }

        long UploadedSize { get; }

        Uri UploadUrl { get; }

        object State { get; }
        
        double UploadPercentage { get; }
    }
}
