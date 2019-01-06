using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace BirdMessenger.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class UploadConfig
    {
        public Uri ServerUrl {get;set;}

        public Uri UploadUrl{get;set;}

        /// <summary>
        /// default size 1mb 
        /// </summary>
        public int MaxChunkSize=1024*1024;

        public FileInfo UploadFile{get;set;}

        /// <summary>
        /// 
        /// </summary>
        public bool IsCancel=false;

        public Dictionary<string,string> UploadMeta{get;set;}

        public delegate void UploadingDel(long bytesFinished,long bytesTotal);

        public Action<long,long> Uploading=null;


        public delegate void PreCreateRequestDel(HttpWebRequest httpWebRequest);

        public Action<HttpWebRequest> PreCreateRequest=null;

        public delegate void PreUploadRequestDel(HttpWebRequest httpWebRequest);

        public Action<HttpWebRequest> PreUploadRequest=null;

        public delegate void UploadFinishDel(Uri uploadFileUrl);

        public Action<Uri> UploadFinish=null;

        public Action<Uri> OnCancel=null;

    }
}