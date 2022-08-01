using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Abstractions;
using BirdMessenger.Collections;
using BirdMessenger.Delegates;
using BirdMessenger.Infrastructure;

namespace BirdMessenger
{
    public class TusClient : ITusClient
    {
        private readonly HttpClient _httpClient;

        public TusClient(HttpClient httpClient)
        {
            if (httpClient is null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            _httpClient = httpClient;

        }

        /// <summary>
        /// create a url for file upload
        /// </summary>
        /// <param name="reqOption"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<TusCreateResponse> TusCreateAsync(TusCreateRequestOption reqOption, CancellationToken ct = default)
        {
            var resp = await _httpClient.TusCreateAsync(reqOption, ct);

            return resp;
        }

        /// <summary>
        /// tus Head request For getting upload-offset
        /// </summary>
        /// <param name="reqOption"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<TusHeadResponse> TusHeadAsync(TusHeadRequestOption reqOption, CancellationToken ct = default)
        {
            var resp = await _httpClient.TusHeadAsync(reqOption, ct);

            return resp;
        }

        /// <summary>
        /// resume upload file
        /// </summary>
        /// <param name="reqOption"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<TusPatchResponse> TusPatchAsync(TusPatchRequestOption reqOption, CancellationToken ct = default)
        {
            var resp = await _httpClient.TusPatchAsync(reqOption, ct);

            return resp;
        }

        /// <summary>
        /// getting tusServer Info
        /// </summary>
        /// <param name="reqOption"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<TusOptionResponse> TusOptionAsync(TusOptionRequestOption reqOption, CancellationToken ct)
        {
            var resp = await _httpClient.TusOptionAsync(reqOption, ct);

            return resp;
        }

        /// <summary>
        /// delete  file
        /// </summary>
        /// <param name="reqOption"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<TusDeleteResponse> TusDeleteAsync(TusDeleteRequestOption reqOption, CancellationToken ct)
        {
            var resp = await _httpClient.TusDeleteAsync(reqOption, ct);

            return resp;
        }
    }
}