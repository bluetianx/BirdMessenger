using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Abstractions;
using BirdMessenger.Core;
using BirdMessenger.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BirdMessenger
{
    public class TusClient
    {
        

        private IServiceProvider _serviceProvider;

        private Uri _serverHost;

        

        /// <summary>
        /// 
        /// return size which will upload
        /// </summary>
        private Func< TusUploadContext,int> GetUploadSize;

        public event Action<TusUploadContext> UploadFinish;

        /// <summary>
        /// uri  offset fileLength 
        /// </summary>
        public event Action<TusUploadContext> Uploading; 

        private  string ClientName { get; set; }


        private  ITusCore _tusCore;
        private ITusExtension _tusExtension;

        public TusClient(IServiceProvider serviceProvider, string clientName,Uri serverHost, Func<TusUploadContext,int> getUploadSize=null)
        {
            _serviceProvider = serviceProvider;
            this.ClientName = clientName;
            _tusCore = serviceProvider.GetRequiredService<ITusCore>();
            _tusExtension = serviceProvider.GetRequiredService<ITusExtension>();
            _tusCore.HttpClientName = clientName;
            _tusExtension.HttpClientName = clientName;
            _serverHost = serverHost;
            GetUploadSize = getUploadSize == null ? (context) => 1 * 1024 * 1024 : getUploadSize;
        }

        public async Task<bool> Upload(Uri url, Stream content, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// upload file
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uploadFileInfo"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<bool> Upload(Uri url,FileInfo uploadFileInfo,CancellationToken ct=default(CancellationToken))
        {
            var headResult = await _tusCore.Head(url, ct);
            long offset = long.Parse(headResult["Upload-Offset"]);

            var tusUploadFileContext = new TusUploadContext(totalSize:uploadFileInfo.Length,
                uploadedSize:offset,uploadFileInfo:uploadFileInfo,uploadFileUrl:url);

            using (var fileStream = new FileStream(uploadFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                while (!ct.IsCancellationRequested)
                {
                    if (offset == uploadFileInfo.Length)
                    {
                        UploadFinish?.Invoke(tusUploadFileContext);
                        break;
                    }
                    
                    //get buffer of file
                    fileStream.Seek (offset, SeekOrigin.Begin);

                    int uploadSize = GetUploadSize(tusUploadFileContext);

                    byte[] buffer = new byte[uploadSize];
                    var readCount = await fileStream.ReadAsync(buffer, 0, uploadSize);
                    if (readCount < uploadSize)
                    {
                        Array.Resize (ref buffer, readCount);
                    }

                    var uploadResult=await _tusCore.Patch(url, buffer, offset, ct);
                    offset = long.Parse(uploadResult["Upload-Offset"]);
                    tusUploadFileContext.UploadedSize = offset;
                    Uploading?.Invoke(tusUploadFileContext);
                }
            }

            return true;
        }

        /// <summary>
        /// create a url for upload file
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="uploadMetaDic"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Uri> Create(FileInfo fileInfo, Dictionary<string, string> uploadMetaDic=null,CancellationToken ct=default(CancellationToken))
        {
            

            string uploadMeta = this.CreateMeta(fileInfo, uploadMetaDic);
            var fileUrl = await _tusExtension.Creation(_serverHost, fileInfo.Length,uploadMeta, ct);

            return fileUrl;
        }

        /// <summary>
        /// delete file
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<bool> DeleteFile(Uri fileUrl, CancellationToken ct=default(CancellationToken))
        {
            var deleteResult = await _tusExtension.Delete(fileUrl, ct);
            return deleteResult;
        }

        public async Task<Dictionary<string, string>> ServerInfo(CancellationToken ct=default(CancellationToken))
        {
            var serverInfoDic = await _tusCore.Options(_serverHost, ct);
            return serverInfoDic;
        }

        private string CreateMeta (FileInfo fileInfo,Dictionary<string, string> uploadMetaDic)
        {
            string uploadMeta = "";

            if (uploadMetaDic == null)
            {
                uploadMetaDic= new Dictionary<string, string>();
            }

            if (!uploadMetaDic.ContainsKey ("fileName"))
            {
                uploadMetaDic["fileName"] = fileInfo.Name;
            }

            List<string> UploadMetaList = new List<string> ();
            foreach (var item in uploadMetaDic)
            {
                string key = item.Key.Replace (" ", "").Replace (",", "");
                string value = Convert.ToBase64String (System.Text.Encoding.UTF8.GetBytes (item.Value));
                UploadMetaList.Add ($"{key} {value}");
            }

            uploadMeta = string.Join (",", UploadMetaList.ToArray ());

            return uploadMeta;
        }

    }
}