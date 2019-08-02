using System;
using System.IO;
using Xunit;
using BirdMessenger;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using BirdMessenger.Abstractions;
using BirdMessenger.Core;
using System.Collections.Generic;

namespace BirdMessenger.Test
{
    public class TusClientUnitTest
    {
        [Fact]
        public async System.Threading.Tasks.Task TestCreateFileAsync()
        {
            string clientName = "tusClient";
            Uri host = new Uri("http://localhost:5000/files");
            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient(clientName, c => 
            {
                c.DefaultRequestHeaders.Add("Tus-Resumable", "1.0.0");
            });

            services.AddTransient<ITusCore, Tus>();
            services.AddTransient<ITusExtension, Tus>();

            var serviceProvider = services.BuildServiceProvider();

            var tusClient = new TusClient(serviceProvider, clientName, host);

            var fileInfo = new FileInfo(@"TestFile/test.mp4");
            Dictionary<string, string> dir = new Dictionary<string, string>();
            dir["filename"] = fileInfo.FullName;

            var result = await tusClient.Create(fileInfo, dir);
            Console.WriteLine(result);
        }
    }
}
