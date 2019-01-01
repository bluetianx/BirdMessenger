using System;
using Xunit;
using BirdMessenger;
using System.Security.Cryptography;

namespace BirdMessenger.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            
        }

        public static bool CompareFileByFilePath(string sourceFile,string targetFile)
        {
            byte[] sourceData = File.ReadAllBytes(sourceFile);
            byte[] targetData = File.ReadAllBytes(targetFile);
            bool resultCompare=false;
            using(SHA256 sHA256=SHA256.Create())
            {
                string hashOfSource = GetHash(sHA256,sourceData);
                resultCompare= VerifyHash(sHA256,targetData,hashOfSource);
            }
            return resultCompare;
        }

        public static bool CompareFileByHash(string hash,string targetFile)
        {
            byte[] data = File.ReadAllBytes(targetFile);
            bool resultCompare=false;
            using(SHA256 sHA256=SHA256.Create())
            {
                resultCompare= VerifyHash(sHA256,hash,data);
            }
            return resultCompare;
        }
        public static string GetHash(HashAlgorithm hashAlgorithm,byte[] data)
        {
            byte[] hashData = hashAlgorithm.ComputeHash(data);

            var sBuilder = new StringBuilder();

            for(int i=0; i<hashData.Length;i++)
            {
                sBuilder.Append(hashData[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static bool VerifyHash(HashAlgorithm hashAlgorithm,byte[] data, string hash)
        {
            string hashOfData = GetHash(hashAlgorithm,data);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hash,hashOfData) == 0;
        }
    }
}
