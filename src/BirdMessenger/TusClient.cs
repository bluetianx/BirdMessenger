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

        public event Action<Uri> UploadFinish;

        public event Action<Uri, long, long> Uploading; 

        private  string ClientName { get; set; }


        private  ITusCore _tusCore;
        private ITusExtension _tusExtension;

        public TusClient(IServiceProvider serviceProvider, string clientName,Uri serverHost)
        {
            _serviceProvider = serviceProvider;
            this.ClientName = clientName;
            _tusCore = serviceProvider.GetRequiredService<ITusCore>();
            _tusExtension = serviceProvider.GetRequiredService<ITusExtension>();
            _tusCore.HttpClientName = clientName;
            _tusExtension.HttpClientName = clientName;
            _serverHost = serverHost;

        }

        public async Task<Uri> Create(FileInfo fileInfo, Dictionary<string, string> uploadMetaDic,CancellationToken requestCancellationToken=null)
        {
            

            string uploadMeta = this.CreateMeta(fileInfo, uploadMetaDic);
            var fileUrl = await _tusExtension.Creation(_serverHost, fileInfo.Length,uploadMeta, requestCancellationToken);

            return fileUrl;
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