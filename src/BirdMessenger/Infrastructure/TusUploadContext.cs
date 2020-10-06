using System;
using System.IO;

namespace BirdMessenger.Infrastructure
{
    internal class TusUploadContext : ITusUploadContext
    {
        public TusUploadContext(long totalSize, long uploadedSize, FileInfo uploadFileInfo, Uri uploadFileUrl)
        {
            TotalSize = totalSize;
            UploadedSize = uploadedSize;
            UploadFileInfo = uploadFileInfo;
            UploadFileUrl = uploadFileUrl;
        }

        public long TotalSize { get; }

        public long UploadedSize { get; set; }

        public double UploadPercentage { get { return (float)UploadedSize / TotalSize; } }

        public FileInfo UploadFileInfo { get; }

        public Uri UploadFileUrl { get; }
    }
}