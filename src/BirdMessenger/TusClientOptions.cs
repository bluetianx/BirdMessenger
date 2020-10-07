using BirdMessenger.Delegates;
using BirdMessenger.Infrastructure;
using System;

namespace BirdMessenger
{
    public class TusClientOptions : ITusClientOptions
    {
        public TusClientOptions()
        {
            FileNameMetadataName = "fileName";
            GetChunkUploadSize = (src, ctx) => 1 * 1024 * 1024;
        }

        /// <summary>
        /// tus server host
        /// </summary>
        public Uri TusHost { get; set; }

        /// <summary>
        /// method to compute the chunk size for upload
        /// </summary>
        public TusChunkUploadSizeDelegate GetChunkUploadSize { get; set; }

        /// <summary>
        /// metadata key for uploaded file name
        /// </summary>
        public string FileNameMetadataName { get; set; }

        public IDisposable ChangeChunkUploadSize(TusChunkUploadSizeDelegate tusChunkUploadSizeDelegate)
        {
            var original = GetChunkUploadSize;
            GetChunkUploadSize = tusChunkUploadSizeDelegate;
            return new TemporaryOptionChange(() =>
            {
                GetChunkUploadSize = original;
            });
        }
    }
}
