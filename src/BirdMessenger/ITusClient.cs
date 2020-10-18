using BirdMessenger.Collections;
using BirdMessenger.Delegates;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BirdMessenger
{
    public interface ITusClient
    {
        /// <summary>
        /// upload completition event
        /// </summary>
        event TusUploadDelegate UploadFinish;

        /// <summary>
        /// upload progress event
        /// </summary>
        event TusUploadDelegate UploadProgress;

        /// <summary>
        /// tus client base options
        /// </summary>
        ITusClientOptions Options { get; }

        /// <summary>
        /// create a url for blob upload
        /// </summary>
        /// <param name="blobLength"></param>
        /// <param name="metadataCollection"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Uri> Create(long blobLength, MetadataCollection metadataCollection = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// create a url for file upload
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="metadataCollection"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Uri> Create(FileInfo fileInfo, MetadataCollection metadataCollection = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// upload blob asynchronously
        /// </summary>
        /// <param name="uploadUrl"></param>
        /// <param name="blobStream"></param>
        /// <param name="state"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Upload(Uri uploadUrl, Stream blobStream, object state, CancellationToken cancellationToken = default);

        /// <summary>
        /// upload file asynchronously
        /// </summary>
        /// <param name="uploadUrl"></param>
        /// <param name="uploadFileInfo"></param>
        /// <param name="state"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Upload(Uri uploadUrl, FileInfo uploadFileInfo, object state, CancellationToken cancellationToken = default);

        /// <summary>
        /// delete file
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> DeleteFile(Uri fileUrl, CancellationToken cancellationToken = default);

        /// <summary>
        /// get server information
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<OptionCollection> ServerInformation(CancellationToken cancellationToken = default);
    }
}