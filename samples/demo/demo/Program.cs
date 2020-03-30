using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using BirdMessenger;
using BirdMessenger.Infrastructure;

namespace demo
{
    class Program
    {
        
        static async Task Main(string[] args)
        {
            FileInfo fileInfo = new FileInfo("test.dmg");
            
            var hostUri = new Uri(@"http://localhost:5000/files");
            var tusClient=TusBuild.DefaultTusClientBuild(hostUri)
                .Build();
            tusClient.Uploading += printUploadProcess;
            tusClient.UploadFinish += UploadFinish;
            Dictionary<string, string> dir = new Dictionary<string, string>();
            dir["filename"] = fileInfo.FullName;

            var fileUrl = await tusClient.Create(fileInfo, dir);
            var uploadResult = await tusClient.Upload(fileUrl, fileInfo);
            Console.ReadLine();
        }

        public static void printUploadProcess(TusUploadContext context)
        {

            Console.WriteLine($"finished:fileUri:{context.UploadFileUrl}-{context.UploadedSize},total:{context.TotalSize} ");
        }

        

        public static void UploadFinish(TusUploadContext context)
        {
            Console.WriteLine($"uploadfinish :{context.UploadFileUrl.ToString()}");
        }

        
    }
}