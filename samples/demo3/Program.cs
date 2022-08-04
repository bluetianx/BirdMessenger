using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
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
            using var stream = new MemoryStream(1024 * 1024 * 32);

            for (var i = 0; i < 1024 * 1024 * 32; i++)
            {
                stream.Write(Encoding.UTF8.GetBytes(BitConverter.ToString(new byte[] { (byte)i }), 0, 2));
            }

            //reset position
            stream.Position = 0;

            // remote tus service
            var tusEndPoint = new Uri(@"http://localhost:5094/files");

            using var httpClient = new HttpClient();
            MetadataCollection metadata = new MetadataCollection();
            metadata["filename"] = "fileName";
            TusCreateRequestOption tusCreateRequestOption = new TusCreateRequestOption()
            {
                Endpoint = tusEndPoint,
                Metadata = metadata,
                UploadLength = stream.Length
            };
            var resp = await httpClient.TusCreateAsync(tusCreateRequestOption, CancellationToken.None);
            bool isInvokeOnProgressAsync = false;
            bool isInvokeOnCompletedAsync = false;
            long uploadedSize = 0;

            TusPatchRequestOption tusPatchRequestOption = new TusPatchRequestOption
            {
                FileLocation = resp.FileLocation,
                Stream = stream,
                OnProgressAsync = x =>
                {
                    isInvokeOnProgressAsync = true;
                    uploadedSize = x.UploadedSize;
                    var uploadedProgress = (int)Math.Floor(100 * (double)x.UploadedSize / x.TotalSize);
                    Console.WriteLine(
                        $"OnProgressAsync-TotalSize:{x.TotalSize}-UploadedSize:{x.UploadedSize}-uploadedProgress:{uploadedProgress}");
                    return Task.CompletedTask;
                },
                OnCompletedAsync = x =>
                {
                    isInvokeOnCompletedAsync = true;
                    var reqOption = x.TusRequestOption as TusPatchRequestOption;
                    Console.WriteLine($"File:{reqOption.FileLocation} Completed ");
                    return Task.CompletedTask;
                },
                OnFailedAsync = x =>
                {
                    Console.WriteLine($"error： {x.Exception.Message}");
                    if (x.OriginHttpRequestMessage is not null)
                    {
                        //log httpRequest
                    }

                    if (x.OriginResponseMessage is not null)
                    {
                        //log response
                    }
                    return Task.CompletedTask;
                }
            };

            var tusPatchResp = await httpClient.TusPatchAsync(tusPatchRequestOption, CancellationToken.None);
            
            // tusPatchResp.OriginResponseMessage
            // tusPatchResp.OriginHttpRequestMessage
        }
    }
}