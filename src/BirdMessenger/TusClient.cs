using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Abstractions;
using BirdMessenger.Configuration;
using BirdMessenger.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BirdMessenger
{
    public class TusClient
    {
        

        private IServiceProvider _serviceProvider;

        private Uri _serverHost;

        

        /// <summary>
        /// first parameter is uploadedSize,second parameter is totalSize
        /// return size which will upload
        /// </summary>
        private Func< long, long,int> UploadSize;

        public event Action<Uri> UploadFinish;

        /// <summary>
        /// uri  offset fileLength 
        /// </summary>
        public event Action<Uri, long, long> Uploading; 

        private  string ClientName { get; set; }


        private  ITusCore _tusCore;
        private ITusExtension _tusExtension;

        public TusClient(IServiceProvider serviceProvider, string clientName,Uri serverHost, Func< long, long,int> uploadSize=null)
        {
            _serviceProvider = serviceProvider;
            this.ClientName = clientName;
            _tusCore = serviceProvider.GetRequiredService<ITusCore>();
            _tusExtension = serviceProvider.GetRequiredService<ITusExtension>();
            _tusCore.HttpClientName = clientName;
            _tusExtension.HttpClientName = clientName;
            _serverHost = serverHost;
            UploadSize = uploadSize == null ? (u, t) => 1 * 1024 * 1024 : uploadSize;
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
            
            using (var fileStream = new FileStream(uploadFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                while (!ct.IsCancellationRequested)
                {
                    if (offset == uploadFileInfo.Length)
                    {
                        UploadFinish?.Invoke(url);
                        break;
                    }
                    
                    //get buffer of file
                    fileStream.Seek (offset, SeekOrigin.Begin);

                    int uploadSize = UploadSize(offset, uploadFileInfo.Length);

                    byte[] buffer = new byte[uploadSize];
                    var readCount = await fileStream.ReadAsync(buffer, 0, uploadSize);
                    if (readCount < uploadSize)
                    {
                        Array.Resize (ref buffer, readCount);
                    }

                    var uploadResult=await _tusCore.Patch(url, buffer, offset, ct);
                    offset = long.Parse(uploadResult["Upload-Offset"]);
                    Uploading?.Invoke(url,offset,uploadFileInfo.Length);
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
        public async Task<Uri> Create(FileInfo fileInfo, Dictionary<string, string> uploadMetaDic,CancellationToken ct=default(CancellationToken))
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