using BirdMessenger.Collections;
using BirdMessenger.Delegates;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BirdMessenger
{
    public interface ITusClient
    {
        /// <summary>
        /// create a url for file upload
        /// </summary>
        /// <param name="reqOption"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TusCreateResponse> TusCreateAsync(TusCreateRequestOption reqOption, CancellationToken ct = default);

        /// <summary>
        /// tus Head request For getting upload-offset
        /// </summary>
        /// <param name="reqOption"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TusHeadResponse> TusHeadAsync(TusHeadRequestOption reqOption, CancellationToken ct = default);

        /// <summary>
        /// resume upload file
        /// </summary>
        /// <param name="reqOption"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TusPatchResponse> TusPatchAsync(TusPatchRequestOption reqOption, CancellationToken ct = default);


        /// <summary>
        /// getting tusServer Info
        /// </summary>
        /// <param name="reqOption"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TusOptionResponse> TusOptionAsync( TusOptionRequestOption reqOption, CancellationToken ct);

        /// <summary>
        /// delete  file
        /// </summary>
        /// <param name="reqOption"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TusDeleteResponse> TusDeleteAsync(TusDeleteRequestOption reqOption, CancellationToken ct);
    }
}