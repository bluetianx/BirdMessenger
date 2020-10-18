using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Abstractions;
using BirdMessenger.Collections;
using BirdMessenger.Delegates;
using BirdMessenger.Infrastructure;

namespace BirdMessenger
{
    public class TusClient : ITusClient
    {
        /// <summary>
        /// upload completition event
        /// </summary>
        public event TusUploadDelegate UploadFinish;

        /// <summary>
        /// upload progress event
        /// </summary>
        public event TusUploadDelegate UploadProgress;

        /// <summary>
        /// tus client options
        /// </summary>
        public ITusClientOptions Options => _tusClientOptions;

        private readonly ITusCore _tusCore;
        private readonly ITusExtension _tusExtension;
        private readonly ITusClientOptions _tusClientOptions;

        public TusClient(ITusCore tusCore, ITusExtension tusExtension, ITusClientOptions tusClientOptions)
        {
            _tusCore = tusCore;
            _tusExtension = tusExtension;
            _tusClientOptions = tusClientOptions;
        }

        /// <summary>
        /// create a url for blobl upload
        /// </summary>
        /// <param name="blobLength"></param>
        /// <param name="metadataContainer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Uri> Create(long blobLength, MetadataCollection metadataContainer = null, CancellationToken cancellationToken = default)
        {
            metadataContainer ??= new MetadataCollection();
            var fileUrl = await _tusExtension.Creation(_tusClientOptions.TusHost, blobLength, metadataContainer.Serialize(), cancellationToken);
            return fileUrl;
        }

        /// <summary>
        /// create a url for file upload
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="metadataContainer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Uri> Create(FileInfo fileInfo, MetadataCollection metadataContainer = null, CancellationToken cancellationToken = default)
        {
            metadataContainer ??= new MetadataCollection();
            if (!metadataContainer.ContainsKey(_tusClientOptions.FileNameMetadataName))
                metadataContainer[_tusClientOptions.FileNameMetadataName] = fileInfo.Name;

            return Create(fileInfo.Length, metadataContainer, cancellationToken);
        }

        /// <summary>
        /// upload blob; will continue from where it left off if a previous upload was already in progress
        /// </summary>
        /// <param name="uploadUrl">blob upload url</param>
        /// <param name="blobStream">blob stream to be uploaded; must allow Length, ReadAsync operations. Seek operation must be available for resumed uploads.</param>
        /// <param name="state"></param>
        /// <param name="cancellationToken">cancellation token to stop the asynchronous action</param>
        /// <returns>Returns true if upload is complete; false otherwise</returns>
        public async Task<bool> Upload(Uri uploadUrl, Stream blobStream, object state, CancellationToken cancellationToken = default)
        {
            var headResult = await _tusCore.Head(uploadUrl, cancellationToken);
            long offset = long.Parse(headResult["Upload-Offset"]);
            long length = blobStream.Length;

            var tusUploadFileContext = new TusUploadContext(length, offset, uploadUrl, state);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (offset == length)
                {
                    UploadFinish?.Invoke(this, tusUploadFileContext);
                    return true;
                }

                if (blobStream.Position != offset)
                    blobStream.Seek(offset, SeekOrigin.Begin);

                int chunkSize = _tusClientOptions.GetChunkUploadSize(this, tusUploadFileContext);
                chunkSize = (int)Math.Min(chunkSize, length - offset);
                byte[] buffer = new byte[chunkSize];
                var readCount = await blobStream.ReadAsync(buffer, 0, chunkSize);

                var uploadResult = await _tusCore.Patch(uploadUrl, buffer, offset, cancellationToken);
                offset = long.Parse(uploadResult["Upload-Offset"]);
                tusUploadFileContext.UploadedSize = offset;
                UploadProgress?.Invoke(this, tusUploadFileContext);
            }
            return false;
        }

        /// <summary>
        /// upload file; will continue from where it left off if a previous upload was already in progress
        /// </summary>
        /// <param name="uploadUrl">file upload url</param>
        /// <param name="uploadFileInfo">file to be uploaded</param>
        /// <param name="state"></param>
        /// <param name="cancellationToken">cancellation token to stop the asynchronous action</param>
        /// <returns>Returns true if upload is complete; false otherwise</returns>
        public async Task<bool> Upload(Uri uploadUrl, FileInfo uploadFileInfo, object state, CancellationToken cancellationToken = default)
        {
            using (var fileStream = new FileStream(uploadFileInfo.FullName, FileMode.Open, FileAccess.Read))
                return await Upload(uploadUrl, fileStream, state, cancellationToken);
        }

        /// <summary>
        /// delete uploaded file
        /// </summary>
        /// <param name="fileUrl">The url provided by #Create</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> DeleteFile(Uri fileUrl, CancellationToken cancellationToken = default)
        {
            return await _tusExtension.Delete(fileUrl, cancellationToken);
        }

        /// <summary>
        /// get server information
        /// </summary>
        public async Task<OptionCollection> ServerInformation(CancellationToken cancellationToken = default)
        {
            return await _tusCore.Options(_tusClientOptions.TusHost, cancellationToken);
        }
    }
}