using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
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
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // file to be uploaded
            FileInfo fileInfo = new FileInfo(Path.Combine(location, "test.txt"));

            // remote tus service
            var hostUri = new Uri(@"http://localhost:5000/files");
            
            // build a standalone tus client instance
            var tusClient = TusBuild.DefaultTusClientBuild(hostUri)
                .Build();

            //hook up events
            tusClient.UploadProgress += printUploadProcess;
            tusClient.UploadFinish += uploadFinish;

            //define additional file metadata 
            MetadataCollection metadata = new MetadataCollection();
            metadata["filename"] = fileInfo.FullName;

            //create upload url
            var fileUrl = await tusClient.Create(fileInfo, metadata);

            //upload file
            var uploadResult = await tusClient.Upload(fileUrl, fileInfo, null);
        }

        public static void printUploadProcess(ITusClient src, ITusUploadContext context)
        {

            Console.WriteLine($"finished:fileUri:{context.UploadUrl}-{context.UploadedSize},total:{context.TotalSize} ");
        }

        public static void uploadFinish(ITusClient src, ITusUploadContext context)
        {
            Console.WriteLine($"uploadfinish :{context.UploadUrl.ToString()}");
        }
    }
}