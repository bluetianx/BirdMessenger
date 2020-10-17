using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using BirdMessenger;
using BirdMessenger.Collections;
using BirdMessenger.Infrastructure;

namespace demo3
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var stream = new MemoryStream(1024 * 1024 * 32);

            for(var i = 0; i < 1024 * 1024 * 32; i++) {
                stream.Write(Encoding.UTF8.GetBytes(BitConverter.ToString(new byte[] { (byte)i }), 0, 2));
            }

            //reset position
            stream.Position = 0;

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

            //create upload url
            var uploadUrl = await tusClient.Create(stream.Length, metadata);

            //upload file
            var uploadResult = await tusClient.Upload(uploadUrl, stream, null);
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