using System;
using System.IO;
using Xunit;
using BirdMessenger;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using BirdMessenger.Collections;

namespace BirdMessenger.Test
{
    public class TusBuildUnitTest
    {
        public Uri tusHost = new Uri("http://localhost:5000/files");

        [Fact]
        public async Task TestCreateTusClientAsync()
        {
            var tusClient = TusBuild.DefaultTusClientBuild(tusHost)

                .Build();
            var fileInfo = new FileInfo(@"TestFile/test.mp4");
            MetadataCollection dir = new MetadataCollection();
            dir["filename"] = fileInfo.FullName;

            var result = await tusClient.Create(fileInfo, dir);
        }

        [Fact]
        public async Task TestUploadFilesAsync()
        {
            var tusClient = TusBuild.DefaultTusClientBuild(tusHost)
                .Build();
            var fileInfo = new FileInfo(@"TestFile/test.mp4");
            MetadataCollection dir = new MetadataCollection();
            dir["filename"] = fileInfo.FullName;
            List<Uri> fileUrls = new List<Uri>();
            for (int i = 0; i < 30; i++)
            {
                var fileUrl = await tusClient.Create(fileInfo, dir);
                fileUrls.Add(fileUrl);
            }

            foreach (var item in fileUrls)
            {
                var uploadResult = await tusClient.Upload(item, fileInfo, null);
                Assert.True(uploadResult);
            }
        }

        [Fact]
        public async Task TestConfigTusAsync()
        {
            var tusClient = TusBuild.DefaultTusClientBuild(tusHost)
                .Configure((option, httpClientBuilder) =>
                {
                    option.GetChunkUploadSize = (s, u) => 10 * 1024 * 1024;
                })
                .Build();
            var fileInfo = new FileInfo(@"TestFile/test.mp4");
            MetadataCollection dir = new MetadataCollection();
            dir["filename"] = fileInfo.FullName;

            var result = await tusClient.Create(fileInfo, dir);
        }

        public static string GetHash(HashAlgorithm hashAlgorithm, byte[] data)
        {
            byte[] hashData = hashAlgorithm.ComputeHash(data);

            var sBuilder = new StringBuilder();

            for (int i = 0; i < hashData.Length; i++)
            {
                sBuilder.Append(hashData[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static bool VerifyHash(HashAlgorithm hashAlgorithm, byte[] data, string hash)
        {
            string hashOfData = GetHash(hashAlgorithm, data);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hash, hashOfData) == 0;
        }
    }
}
