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
        /// upload file; will continue from where it left off if a previous upload was already in progress
        /// </summary>
        /// <param name="fileUrl">file upload url</param>
        /// <param name="uploadFileInfo">file to be uploaded</param>
        /// <param name="cancellationToken">cancellation token to stop the asynchronous action</param>
        /// <returns>Returns true if upload is complete; false otherwise</returns>
        public async Task<bool> Upload(Uri fileUrl, FileInfo uploadFileInfo, CancellationToken cancellationToken = default)
        {
            var headResult = await _tusCore.Head(fileUrl, cancellationToken);
            long offset = long.Parse(headResult["Upload-Offset"]);

            var tusUploadFileContext = new TusUploadContext(totalSize: uploadFileInfo.Length,
                uploadedSize: offset, uploadFileInfo: uploadFileInfo, uploadFileUrl: fileUrl);

            using (var fileStream = new FileStream(uploadFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (offset == uploadFileInfo.Length)
                    {
                        UploadFinish?.Invoke(this, tusUploadFileContext);
                        return true;
                    }

                    //get buffer of file
                    fileStream.Seek(offset, SeekOrigin.Begin);

                    int chunkSize = _tusClientOptions.GetChunkUploadSize(this, tusUploadFileContext);
                    chunkSize = (int)Math.Min(chunkSize, fileStream.Length - offset);
                    byte[] buffer = new byte[chunkSize];
                    var readCount = await fileStream.ReadAsync(buffer, 0, chunkSize);

                    var uploadResult = await _tusCore.Patch(fileUrl, buffer, offset, cancellationToken);
                    offset = long.Parse(uploadResult["Upload-Offset"]);
                    tusUploadFileContext.UploadedSize = offset;
                    UploadProgress?.Invoke(this, tusUploadFileContext);
                }
            }
            return false;
        }

        /// <summary>
        /// create a url for upload file
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="metadataContainer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Uri> Create(FileInfo fileInfo, MetadataCollection metadataContainer = null, CancellationToken cancellationToken = default)
        {
            string uploadMeta = createMeta(fileInfo, metadataContainer);
            var fileUrl = await _tusExtension.Creation(_tusClientOptions.TusHost, fileInfo.Length, uploadMeta, cancellationToken);
            return fileUrl;
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

        public async Task<OptionCollection> ServerInfo(CancellationToken cancellationToken = default)
        {
            var serverInfoDic = await _tusCore.Options(_tusClientOptions.TusHost, cancellationToken);
            return serverInfoDic;
        }

        private string createMeta(FileInfo fileInfo, MetadataCollection metadataContainer)
        {
            if (metadataContainer == null)
                metadataContainer = new MetadataCollection();

            if (!metadataContainer.ContainsKey(_tusClientOptions.FileNameMetadataName))
                metadataContainer[_tusClientOptions.FileNameMetadataName] = fileInfo.Name;

            List<string> uploadMetaList = new List<string>();
            foreach (var item in metadataContainer)
            {
                string key = item.Key;
                string value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(item.Value));
                uploadMetaList.Add($"{key} {value}");
            }

            return string.Join(",", uploadMetaList.ToArray());
        }
    }
}