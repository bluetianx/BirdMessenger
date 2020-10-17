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
using System.Threading.Tasks;
using BirdMessenger.Collections;

namespace BirdMessenger.Test
{
    public class TusClientUnitTest
    {
        [Fact]
        public async Task TestCreateFileAsync()
        {
            var tusClient = this.BuildClient();

            var fileInfo = new FileInfo(@"TestFile/testf");
            MetadataCollection dir = new MetadataCollection();
            dir["filename"] = fileInfo.FullName;

            var result = await tusClient.Create(fileInfo, dir);
        }

        [Fact]
        public async Task TestUploadFileAsync()
        {
            var tusClient = this.BuildClient();
            var fileInfo = new FileInfo(@"TestFile/test.mp4");
            MetadataCollection dir = new MetadataCollection();

            var fileUrl = await tusClient.Create(fileInfo, dir);
            var uploadResult = await tusClient.Upload(fileUrl, fileInfo, null);
        }

        [Fact]
        public async Task TestDeleteFileAsync()
        {
            var tusClient = this.BuildClient();
            var fileInfo = new FileInfo(@"TestFile/test.mp4");
            MetadataCollection dir = new MetadataCollection();

            var fileUrl = await tusClient.Create(fileInfo, dir);

            var deleteResult = await tusClient.DeleteFile(fileUrl);
        }

        [Fact]
        public async Task TestServiceInfoAsync()
        {
            var tusClient = this.BuildClient();

            var serviceInfo = await tusClient.ServerInformation();
        }

        private ITusClient BuildClient()
        {
            Uri host = new Uri("http://localhost:6000/files");

            ITusClient tusClient = TusBuild.DefaultTusClientBuild(host)
                .Build();

            return tusClient;
        }
    }
}
