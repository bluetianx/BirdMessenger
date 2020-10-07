using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Abstractions;
using BirdMessenger.Collections;
using BirdMessenger.Infrastructure;

namespace BirdMessenger.Core
{
    /// <summary>
    /// Tus implementation class
    /// </summary>
    public class Tus : ITusCore, ITusExtension
    {
        private HttpClient _httpClient;

        public Tus(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="requestCancellationToken"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> Head(Uri url, CancellationToken requestCancellationToken)
        {
            var httpReqMsg = new HttpRequestMessage(HttpMethod.Head, url);

            var response = await _httpClient.SendAsync(httpReqMsg, requestCancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound
                || response.StatusCode == HttpStatusCode.Gone
                || response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new TusException($"response's statusCode is{response.StatusCode.ToString()} ");
            }

            Dictionary<string, string> result = new Dictionary<string, string>();
            result["Upload-Offset"] = response.GetValueOfHeader("Upload-Offset");
            result["Tus-Resumable"] = response.GetValueOfHeader("Tus-Resumable");

            return result;
        }

        public async Task<Dictionary<string, string>> Patch(Uri url, byte[] uploadData, long offset,
            CancellationToken requestCancellationToken)
        {
            var httpReqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            httpReqMsg.Headers.Add("Upload-Offset", offset.ToString());
            //httpReqMsg.Headers.Add("Content-Type","application/offset+octet-stream");

            httpReqMsg.Content = new ByteArrayContent(uploadData);
            httpReqMsg.Content.Headers.Add("Content-Type", "application/offset+octet-stream");

            var response = await _httpClient.SendAsync(httpReqMsg, requestCancellationToken);
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new TusException($"patch response statusCode is {response.StatusCode.ToString()}");
            }
            Dictionary<string, string> result = new Dictionary<string, string>();
            result["Upload-Offset"] = response.GetValueOfHeader("Upload-Offset");
            result["Tus-Resumable"] = response.GetValueOfHeader("Tus-Resumable");

            return result;
        }

        public async Task<OptionCollection> Options(Uri url, CancellationToken requestCancellationToken)
        {
            var httpReqMsg = new HttpRequestMessage(HttpMethod.Options, url);

            if (_httpClient.DefaultRequestHeaders.Contains("Tus-Resumable"))
            {
                _httpClient.DefaultRequestHeaders.Remove("Tus-Resumable");
            }

            var response = await _httpClient.SendAsync(httpReqMsg, requestCancellationToken);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new TusException($"Options response statusCode is {response.StatusCode}");
            }

            OptionCollection result = new OptionCollection();
            result["Tus-Version"] = response.GetValueOfHeader("Tus-Version");
            result["Tus-Resumable"] = response.GetValueOfHeader("Tus-Resumable");
            
            if (response.Headers.Contains("Tus-Extension"))
                result["Tus-Extension"] = response.GetValueOfHeader("Tus-Extension");

            if (response.Headers.Contains("Tus-Max-Size"))
                result["Tus-Max-Size"] = response.GetValueOfHeader("Tus-Max-Size");
            
            return result;
        }

        public async Task<Uri> Creation(Uri url, long uploadLength, string uploadMetadata, CancellationToken requestCancellationToken)
        {
            var httpReqMsg = new HttpRequestMessage(HttpMethod.Post, url);
            httpReqMsg.Headers.Add("Upload-Length", uploadLength.ToString());
            if (!string.IsNullOrEmpty(uploadMetadata))
            {
                httpReqMsg.Headers.Add("Upload-Metadata", uploadMetadata);
            }

            var response = await _httpClient.SendAsync(httpReqMsg, requestCancellationToken);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new TusException($"creation response statusCode is {response.StatusCode}");
            }

            string fileUrlStr = response.GetValueOfHeader("Location");
            Uri fileUrl = null;
            if (Uri.TryCreate(fileUrlStr, UriKind.RelativeOrAbsolute, out fileUrl))
            {
                if (!fileUrl.IsAbsoluteUri)
                {
                    fileUrl = new Uri(url, fileUrl);
                }
            }
            else
            {
                throw new Exception("Invalid location header");
            }
            return fileUrl;
        }

        public async Task<bool> Delete(Uri url, CancellationToken requestCancellationToken)
        {
            var httpReqMsg = new HttpRequestMessage(HttpMethod.Delete, url);

            var response = await _httpClient.SendAsync(httpReqMsg, requestCancellationToken);
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new TusException($"delete response statusCode is {response.StatusCode}");
            }


            return true;
        }
    }
}