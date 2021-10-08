using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly HttpClient _httpClient;

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
            return await CreateFileAsync(url, uploadLength, uploadMetadata, null,requestCancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uploadLength">if uploadLength less than 0, http does not include header of Upload-Length</param>
        /// <param name="uploadMetadata"></param>
        /// <param name="headers"></param>
        /// <param name="requestCancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="TusException"></exception>
        /// <exception cref="Exception"></exception>
        private async Task<Uri> CreateFileAsync(Uri url, long uploadLength, string uploadMetadata,IDictionary<string,string> headers,
            CancellationToken requestCancellationToken =default)
        {
            var httpReqMsg = new HttpRequestMessage(HttpMethod.Post, url);
            if (uploadLength >= 0)
            {
                httpReqMsg.Headers.Add("Upload-Length", uploadLength.ToString());
            }
            
            if (!string.IsNullOrEmpty(uploadMetadata))
            {
                httpReqMsg.Headers.Add("Upload-Metadata", uploadMetadata);
            }

            if (headers is not null && headers.Any())
            {
                foreach (var key in headers.Keys)
                {
                    httpReqMsg.Headers.Add(key,headers[key]);
                }
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
                if (fileUrlStr.StartsWith("https://") || fileUrlStr.StartsWith("http://"))
                    return fileUrl;
                fileUrl = new Uri(url, fileUrl);
            }
            else
            {
                throw new Exception("Invalid location header");
            }

            return fileUrl;
        }

        public async Task<Uri> CreatePartialAsync(Uri host, long uploadLength, CancellationToken ct = default)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers["Upload-Concat"] = "partial";
            var fileUrl = await CreateFileAsync(host, uploadLength, string.Empty, headers, ct);
            return fileUrl;
        }

        public async Task<Uri> ConcatenateAsync(Uri host, string[] partialFiles, CancellationToken ct = default)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            var filesStr = string.Join(" ", partialFiles);
            headers["Upload-Concat"] = $"final;{filesStr}";
            var finalFileUrl = await CreateFileAsync(host, -1,string.Empty, headers, ct);
            return finalFileUrl;
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

        public Task<Dictionary<string, string>> CreationWithUploadAsync(Uri url, long uploadLength, string uploadMetadata, byte[] uploadData,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}