﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using BirdMessenger;
using BirdMessenger.Collections;
using BirdMessenger.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // file to be uploaded
            FileInfo fileInfo = new FileInfo(Path.Combine(location, @"TestFile/test.txt"));

            // remote tus service
            var hostUri = new Uri(@"http://localhost:6000/files");
            
            // build a standalone tus client instance
           /* var tusClient = null;
            //hook up events
            tusClient.UploadProgress += printUploadProcess;
            tusClient.UploadFinish += uploadFinish;

            //define additional file metadata 
            MetadataCollection metadata = new MetadataCollection();
            metadata["filename"] = fileInfo.FullName;
            
            TusRequestOption requestOption = new TusRequestOption();
            requestOption.HttpHeader["hello"] = "hello";
            
            //create upload url
            var fileUrl = await tusClient.Create(fileInfo,null,requestOption);

            var uploadOpt = new TusRequestOption()
            {
                UploadWithStreaming = true //enable streaming Upload
            };

            //upload file
            var uploadResult = await tusClient.Upload(fileUrl, fileInfo, null,uploadOpt);*/
        }
        
    }
}