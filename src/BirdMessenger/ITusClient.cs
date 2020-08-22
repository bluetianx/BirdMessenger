using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Infrastructure;

namespace BirdMessenger
{
    public interface ITusClient
    {
        event Action<TusUploadContext> UploadFinish;

        /// <summary>
        /// uri  offset fileLength 
        /// </summary>
        event Action<TusUploadContext> Uploading;
        
        
        /// <summary>
        /// create a url for upload file
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="uploadMetaDic"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Uri> Create(FileInfo fileInfo, Dictionary<string, string> uploadMetaDic = null,
            CancellationToken ct = default(CancellationToken));
        
        /// <summary>
        /// upload file
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uploadFileInfo"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> Upload(Uri url, FileInfo uploadFileInfo, CancellationToken ct = default(CancellationToken));
        
        /// <summary>
        /// delete file
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> DeleteFile(Uri fileUrl, CancellationToken ct = default(CancellationToken));

        /// <summary>
        /// get serverInfo
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> ServerInfo(CancellationToken ct = default(CancellationToken));
    }
}