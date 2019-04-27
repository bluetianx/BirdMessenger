using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Abstractions;
using BirdMessenger.Infrastructure;

namespace BirdMessenger.Core
{
    
    /// <summary>
    /// Tus implementation class
    /// </summary>
    public class Tus:ITusCore,ITusExtension
    {

        private readonly IHttpClientFactory _httpClientFactory;
        
        public CancellationToken RequestCancellationToken { get; set; }
        
        public  string HttpClientName { get; set; }

        public Tus(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetHttpClient()
        {
            var client = string.IsNullOrEmpty(HttpClientName)
                ? this._httpClientFactory.CreateClient()
                : this._httpClientFactory.CreateClient(HttpClientName);

            return client;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> Head(Uri url)
        {
            var httpReqMsg = new HttpRequestMessage(HttpMethod.Head, url);
            var client = this.GetHttpClient();
            var response = await client.SendAsync(httpReqMsg, RequestCancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound 
                || response.StatusCode == HttpStatusCode.Gone
                || response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw  new TusException($"response's statusCode is{response.StatusCode.ToString()} ");
            }
            
            Dictionary<string,string> result = new Dictionary<string, string>();
            result["Upload-Offset"] = response.GetValueOfHeader("Upload-Offset");
            result["Tus-Resumable"] = response.GetValueOfHeader("Tus-Resumable");
            
            return result;
        }

        public async Task<Dictionary<string, string>> Patch(Uri url, byte[] uploadData, int offset)
        {
            var httpReqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            httpReqMsg.Headers.Add("Upload-Offset",offset.ToString());
            httpReqMsg.Headers.Add("Content-Type","application/offset+octet-stream");
            httpReqMsg.Content = new ByteArrayContent(uploadData);
            
            var client = this.GetHttpClient();
            var response = await client.SendAsync(httpReqMsg, RequestCancellationToken);
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new TusException($"patch response statusCode is {response.StatusCode.ToString()}");
            }
            Dictionary<string,string> result = new Dictionary<string, string>();
            result["Upload-Offset"] = response.GetValueOfHeader("Upload-Offset");
            result["Tus-Resumable"] = response.GetValueOfHeader("Tus-Resumable");
            
            return result;
        }

        public async Task<Dictionary<string, string>> Options(Uri url)
        {
            var httpReqMsg = new HttpRequestMessage(HttpMethod.Options, url);
            
            var client = this.GetHttpClient();
            if (client.DefaultRequestHeaders.Contains("Tus-Resumable"))
            {
                client.DefaultRequestHeaders.Remove("Tus-Resumable");
            }

            var response = await client.SendAsync(httpReqMsg, RequestCancellationToken);
            if (response.StatusCode != HttpStatusCode.OK || response.StatusCode != HttpStatusCode.NoContent)
            {
                throw  new TusException($"Options response statusCode is {response.StatusCode.ToString()}");
            }
            
            Dictionary<string,string> result = new Dictionary<string, string>();
            result["Tus-Version"] = response.GetValueOfHeader("Tus-Version");
            result["Tus-Resumable"] = response.GetValueOfHeader("Tus-Resumable");
            if (response.Headers.Contains("Tus-Extension"))
            {
                result["Tus-Extension"] = response.GetValueOfHeader("Tus-Extension");
            }

            if (response.Headers.Contains("Tus-Max-Size "))
            {
                result["Tus-Max-Size "] = response.GetValueOfHeader("Tus-Max-Size ");
            }
            return result;
        }

        public Uri Creation(Uri url, int uploadLength, string uploadMetadata)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Uri url)
        {
            throw new NotImplementedException();
        }
    }
}