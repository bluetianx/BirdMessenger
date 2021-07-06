using BirdMessenger.Abstractions;
using BirdMessenger.Delegates;
using System;

namespace BirdMessenger
{


    public interface ITusClientOptions
    {
        /// <summary>
        /// tus server host
        /// </summary>
        public Uri TusHost { get; }

        /// <summary>
        /// method to compute the chunk size for upload
        /// </summary>
        public TusChunkUploadSizeDelegate GetChunkUploadSize { get; }

        /// <summary>
        /// generate temporary change
        /// </summary>
        public IDisposable ChangeChunkUploadSize(TusChunkUploadSizeDelegate tusChunkUploadSizeDelegate);

        /// <summary>
        /// metadata key for uploaded file name
        /// </summary>
        public string FileNameMetadataName { get; }

        /// <summary>
        /// Resume
        /// </summary>
        public bool Resume { get; set; }

        /// <summary>
        /// Store
        /// </summary>
        public IStore Store { get; set; }

    }
}
