using System;
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

        public void UploadFile()
        {
            _uploader.UploadFile();
        }

    }
}