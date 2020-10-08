using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using BirdMessenger;
using BirdMessenger.Collections;
using BirdMessenger.Infrastructure;

namespace demo
{
    class Program
    {

        static async Task Main(string[] args)
        {
            FileInfo fileInfo = new FileInfo("test.txt");

            var hostUri = new Uri(@"http://localhost:5000/files");
            var tusClient = TusBuild.DefaultTusClientBuild(hostUri)
                .Build();
            tusClient.UploadProgress += printUploadProcess;
            tusClient.UploadFinish += uploadFinish;
            MetadataCollection metadata = new MetadataCollection();
            metadata["filename"] = fileInfo.FullName;

            var fileUrl = await tusClient.Create(fileInfo, metadata);
            var uploadResult = await tusClient.Upload(fileUrl, fileInfo);
            Console.ReadLine();
        }

        public static void printUploadProcess(ITusClient src, ITusUploadContext context)
        {

            Console.WriteLine($"finished:fileUri:{context.UploadFileUrl}-{context.UploadedSize},total:{context.TotalSize} ");
        }

        public static void uploadFinish(ITusClient src, ITusUploadContext context)
        {
            Console.WriteLine($"uploadfinish :{context.UploadFileUrl.ToString()}");
        }
    }
}