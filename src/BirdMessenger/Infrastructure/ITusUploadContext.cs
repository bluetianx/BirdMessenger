using System;

namespace BirdMessenger.Infrastructure
{
    public interface ITusUploadContext
    {
        long TotalSize { get; }

        long UploadedSize { get; }

        Uri UploadUrl { get; }

        object State { get; }

        double UploadPercentage { get; }

        /// <summary>
        /// Name-Size-{ChangedTime}
        /// </summary>
        string FingerPrint { get; }
    }
}
