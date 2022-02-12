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
        /// create a url for blob upload
        /// </summary>
        /// <param name="blobLength"></param>
        /// <param name="metadataContainer"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Uri> Create(long blobLength, MetadataCollection metadataContainer = null,TusRequestOption option=default, CancellationToken ct = default)
        {
            metadataContainer ??= new MetadataCollection();
            var fileUrl = await _tusExtension.Creation(_tusClientOptions.TusHost, blobLength, metadataContainer.Serialize(),option, ct);
            return fileUrl;
        }

        /// <summary>
        /// create a url for file upload
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="metadataContainer"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public Task<Uri> Create(FileInfo fileInfo, MetadataCollection metadataContainer = null,TusRequestOption option=default, CancellationToken ct = default)
        {
            metadataContainer ??= new MetadataCollection();
            if (!metadataContainer.ContainsKey(_tusClientOptions.FileNameMetadataName))
                metadataContainer[_tusClientOptions.FileNameMetadataName] = fileInfo.Name;

            return Create(fileInfo.Length, metadataContainer,option, ct);
        }

        /// <summary>
        /// upload blob; will continue from where it left off if a previous upload was already in progress
        /// </summary>
        /// <param name="uploadUrl">blob upload url</param>
        /// <param name="blobStream">blob stream to be uploaded; must allow Length, ReadAsync operations. Seek operation must be available for resumed uploads.</param>
        /// <param name="state"></param>
        /// <param name="option"></param>
        /// <param name="ct">cancellation token to stop the asynchronous action</param>
        /// <returns>Returns true if upload is complete; false otherwise</returns>
        public async Task<bool> Upload(Uri uploadUrl, Stream blobStream, object state,TusRequestOption option=default, CancellationToken ct = default)
        {
            bool uploadResult;
            if (option is not null && option.UploadWithStreaming)
            {
                uploadResult = await UploadWithStreamingAsync(uploadUrl, blobStream, state, option, ct);
            }
            else
            {
                uploadResult = await UploadWithMultipleRequest(uploadUrl, blobStream, state, option, ct);
            }
            return uploadResult;
        }

        private async Task<bool> UploadWithMultipleRequest(Uri uploadUrl, Stream blobStream, object state, TusRequestOption option,
            CancellationToken ct)
        {
            var headResult = await _tusCore.Head(uploadUrl, option, ct);
            long offset = long.Parse(headResult["Upload-Offset"]);
            long length = blobStream.Length;

            var tusUploadFileContext = new TusUploadContext(length, offset, uploadUrl, state);

            while (!ct.IsCancellationRequested)
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

                var uploadResult = await _tusCore.Patch(uploadUrl, buffer, offset, option, ct);
                offset = long.Parse(uploadResult["Upload-Offset"]);
                tusUploadFileContext.UploadedSize = offset;
                UploadProgress?.Invoke(this, tusUploadFileContext);
            }

            return false;
        }

        private async Task<bool> UploadWithStreamingAsync(Uri uploadUrl, Stream blobStream, object state,
            TusRequestOption option = default, CancellationToken ct = default)
        {
            var headResult = await _tusCore.Head(uploadUrl,option, ct);
            long offset = long.Parse(headResult["Upload-Offset"]);
            long length = blobStream.Length;

            var tusUploadFileContext = new TusUploadContext(length, offset, uploadUrl, state);
            if (offset == length)
            {
                UploadFinish?.Invoke(this, tusUploadFileContext);
                return true;
            }

            if (blobStream.Position != offset)
            {
                blobStream.Seek(offset, SeekOrigin.Begin);
            }

            var uploadResult = await _tusCore.PatchWithStreaming(uploadUrl, blobStream, OnUploadProgress, option, ct);
            var uploadedBytes = long.Parse(uploadResult["Upload-Offset"]);

            Task OnUploadProgress(long offset)
            {
                tusUploadFileContext.UploadedSize = offset;
                UploadProgress?.Invoke(this, tusUploadFileContext);
                return Task.CompletedTask;
            }

            return uploadedBytes == length;
        }

        /// <summary>
        /// upload file; will continue from where it left off if a previous upload was already in progress
        /// </summary>
        /// <param name="uploadUrl">file upload url</param>
        /// <param name="uploadFileInfo">file to be uploaded</param>
        /// <param name="state"></param>
        /// <param name="cancellationToken">cancellation token to stop the asynchronous action</param>
        /// <returns>Returns true if upload is complete; false otherwise</returns>
        public async Task<bool> Upload(Uri uploadUrl, FileInfo uploadFileInfo, object state,TusRequestOption option=default, CancellationToken cancellationToken = default)
        {
            using var fileStream = new FileStream(uploadFileInfo.FullName, FileMode.Open, FileAccess.Read);
            return await Upload(uploadUrl, fileStream, state,option, cancellationToken);
        }

        /// <summary>
        /// delete uploaded file
        /// </summary>
        /// <param name="fileUrl">The url provided by #Create</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> DeleteFile(Uri fileUrl,TusRequestOption option =default, CancellationToken ct = default)
        {
            return await _tusExtension.Delete(fileUrl,option, ct);
        }

        /// <summary>
        /// get server information
        /// </summary>
        public async Task<OptionCollection> ServerInformation(TusRequestOption option=default,CancellationToken ct = default)
        {
            return await _tusCore.Options(_tusClientOptions.TusHost,option, ct);
        }
    }
}