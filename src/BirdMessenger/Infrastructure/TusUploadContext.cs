using System;
using System.IO;

namespace BirdMessenger.Infrastructure
{
    internal class TusUploadContext : ITusUploadContext
    {
        public TusUploadContext(long totalSize, long uploadedSize, Uri uploadUrl, object state)
        {
            TotalSize = totalSize;
            UploadedSize = uploadedSize;
            UploadUrl = uploadUrl;
            State = state;
        }

        public long TotalSize { get; }

        public long UploadedSize { get; set; }

        public Uri UploadUrl { get; }

        public object State { get; }
        
        public double UploadPercentage { get { return (float)UploadedSize / TotalSize; } }
    }
}