using BirdMessenger.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BirdMessenger.Abstractions
{
    /// <summary>
    /// Tus core Protocol
    /// </summary>
    public interface ITusCore
    {
        /// <summary>
        /// tus head request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> Head(Uri url,TusRequestOption option=default, CancellationToken cancellationToken = default);

        /// <summary>
        /// tus patch request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uploadData"></param>
        /// <param name="offset"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> Patch(Uri url, byte[] uploadData, long offset,TusRequestOption option=default, CancellationToken cancellationToken = default);

        /// <summary>
        /// upload file with streaming
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uploadStream"></param>
        /// <param name="uploadProgress"></param>
        /// <param name="option"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> PatchWithStreaming(Uri url, Stream uploadStream, Func<long, Task> uploadProgress,
            TusRequestOption option = default, CancellationToken ct = default);
        /// <summary>
        /// tus options request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<OptionCollection> Options(Uri url,TusRequestOption option=default, CancellationToken cancellationToken = default);
    }
}