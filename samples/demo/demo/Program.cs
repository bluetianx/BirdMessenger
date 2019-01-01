using System;
using System.IO;
using System.Net;

using BirdMessenger;
using BirdMessenger.Configuration;
namespace demo
{
    class Program
    {
        public static UploadConfig uploadConfig;
        static void Main(string[] args)
        {
            FileInfo fileInfo = new FileInfo("test.dmg");
            uploadConfig =new UploadConfig();
            uploadConfig.ServerUrl = new Uri(@"http://localhost:1080/uploads");
            uploadConfig.UploadFile= fileInfo;
            uploadConfig.Uploading=printUploadProcess;
            uploadConfig.PreCreateRequest=preCreateFile;
            uploadConfig.PreUploadRequest= preUploadFile;
            uploadConfig.UploadFinish=UploadFinish;
            uploadConfig.OnCancel=Cancel;
            TusClient  tusClient = new TusClient(uploadConfig);

            var url = tusClient.Create();

            tusClient.UploadFile();

        }

        public static void printUploadProcess(long offset,long total)
        {

            Console.WriteLine($"finished:{offset},total:{total} ");
            uploadConfig.IsCancel=true;
        }

        public static void preCreateFile(HttpWebRequest httpWebRequest)
        {
            Console.WriteLine("starting createFile...");
        }

        public static void preUploadFile(HttpWebRequest httpWebRequest)
        {
            Console.WriteLine("starting upLoadFile...");
        }

        public static void UploadFinish(Uri url)
        {
            Console.WriteLine($"uploadfinish :{url.ToString()}");
        }

        public static void Cancel(Uri url)
        {
            Console.WriteLine($"{url.ToString()} canceled...");
        }
    }
}