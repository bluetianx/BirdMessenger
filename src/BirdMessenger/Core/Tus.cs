using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Abstractions;
using BirdMessenger.Collections;
using BirdMessenger.Infrastructure;
using BirdMessenger.Internal;

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
        public async Task<Dictionary<string, string>> Head(Uri url,TusRequestOption option=default, CancellationToken ct=default)
        {
            var httpReqMsg = new HttpRequestMessage(HttpMethod.Head, url);
            ConfigHttpRequestMsg(option, httpReqMsg);
            var response = await _httpClient.SendAsync(httpReqMsg, ct);

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

        private static void ConfigHttpRequestMsg(TusRequestOption option, HttpRequestMessage httpReqMsg)
        {
            if (option is not null)
            {
                option.Validate();
                foreach (var kv in option.HttpHeader)
                {
                    httpReqMsg.Headers.Add(kv.Key, kv.Value);
                }
            }
        }

        public async Task<Dictionary<string, string>> Patch(Uri url, byte[] uploadData, long offset,
            TusRequestOption option=default,CancellationToken ct=default)
        {
            var httpReqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            httpReqMsg.Headers.Add("Upload-Offset", offset.ToString());
            ConfigHttpRequestMsg(option, httpReqMsg);

            httpReqMsg.Content = new ByteArrayContent(uploadData);
            httpReqMsg.Content.Headers.Add("Content-Type", "application/offset+octet-stream");

            var response = await _httpClient.SendAsync(httpReqMsg, ct);
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new TusException($"patch response statusCode is {response.StatusCode.ToString()}");
            }
            Dictionary<string, string> result = new Dictionary<string, string>();
            result["Upload-Offset"] = response.GetValueOfHeader("Upload-Offset");
            result["Tus-Resumable"] = response.GetValueOfHeader("Tus-Resumable");

            return result;
        }

        /// <summary>
        /// upload file with streaming
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uploadStream"></param>
        /// <param name="uploadProgress"></param>
        /// <param name="option"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> PatchWithStreaming(Uri url, Stream uploadStream, Func<long, Task> uploadProgress, TusRequestOption option = default,
            CancellationToken ct = default)
        {
            var httpReqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            httpReqMsg.Headers.Add("Upload-Offset", uploadStream.Position.ToString());
            ConfigHttpRequestMsg(option, httpReqMsg);

            httpReqMsg.Content = new ProgressableStreamContent(uploadStream, uploadProgress);
            httpReqMsg.Content.Headers.Add("Content-Type", "application/offset+octet-stream");
            var response = await _httpClient.SendAsync(httpReqMsg, ct);
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new TusException($"patch response statusCode is {response.StatusCode.ToString()}",httpReqMsg,response);
            }
            Dictionary<string, string> result = new Dictionary<string, string>();
            result["Upload-Offset"] = response.GetValueOfHeader("Upload-Offset");
            result["Tus-Resumable"] = response.GetValueOfHeader("Tus-Resumable");

            return result;
        }

        public async Task<OptionCollection> Options(Uri url, TusRequestOption option=default,CancellationToken ct=default)
        {
            var httpReqMsg = new HttpRequestMessage(HttpMethod.Options, url);
            ConfigHttpRequestMsg(option, httpReqMsg);

            if (_httpClient.DefaultRequestHeaders.Contains("Tus-Resumable"))
            {
                _httpClient.DefaultRequestHeaders.Remove("Tus-Resumable");
            }

            var response = await _httpClient.SendAsync(httpReqMsg, ct);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new TusException($"Options response statusCode is {response.StatusCode}",httpReqMsg,response);
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

        public async Task<Uri> Creation(Uri url, long uploadLength, string uploadMetadata,TusRequestOption option=default, CancellationToken ct=default)
        {
            return await CreateFileAsync(url, uploadLength, uploadMetadata, null,option,ct);
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
            TusRequestOption option=default,CancellationToken ct =default)
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
            ConfigHttpRequestMsg(option, httpReqMsg);

            var response = await _httpClient.SendAsync(httpReqMsg, ct);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new TusException($"creation response statusCode is {response.StatusCode}",httpReqMsg,response);
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

        public async Task<Uri> CreatePartialAsync(Uri host, long uploadLength,TusRequestOption option=default, CancellationToken ct = default)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers["Upload-Concat"] = "partial";
            var fileUrl = await CreateFileAsync(host, uploadLength, string.Empty, headers,option, ct);
            return fileUrl;
        }

        public async Task<Uri> ConcatenateAsync(Uri host, string[] partialFiles,TusRequestOption option=default, CancellationToken ct = default)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            var filesStr = string.Join(" ", partialFiles);
            headers["Upload-Concat"] = $"final;{filesStr}";
            var finalFileUrl = await CreateFileAsync(host, -1,string.Empty, headers,option, ct);
            return finalFileUrl;
        }


        public async Task<bool> Delete(Uri url,TusRequestOption option=default, CancellationToken ct=default)
        {
            var httpReqMsg = new HttpRequestMessage(HttpMethod.Delete, url);
            ConfigHttpRequestMsg(option, httpReqMsg);
            
            var response = await _httpClient.SendAsync(httpReqMsg, ct);
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new TusException($"delete response statusCode is {response.StatusCode}",httpReqMsg,response);
            }
            return true;
        }

        public Task<Dictionary<string, string>> CreationWithUploadAsync(Uri url, long uploadLength, string uploadMetadata, byte[] uploadData,
            TusRequestOption option=default,CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}