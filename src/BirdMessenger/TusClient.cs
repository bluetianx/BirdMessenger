using System;
using System.Collections.Generic;
using BirdMessenger.Configuration;
using BirdMessenger.Core;

namespace BirdMessenger
{
    public class TusClient
    {
        public UploadConfig _UploadConfig;

        private Uploader _uploader;

        public TusClient(UploadConfig uploadConfig)
        {
            _UploadConfig = uploadConfig;
            _uploader = new Uploader(uploadConfig);
        }

        public Uri Create()
        {
            _uploader.Create();
            return _UploadConfig.UploadUrl;
        }

        public Dictionary<string,string> GetServerInfo()
        {
            return _uploader.GetServerInfo();
        }
        public void UploadFile()
        {
            _uploader.UploadFile();
        }

    }
}