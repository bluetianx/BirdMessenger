using BirdMessenger.Collections;
using System;
using System.Collections.Generic;
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
        /// <param name="requestCancellationToken"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> Head(Uri url, CancellationToken requestCancellationToken);

        /// <summary>
        /// tus patch request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uploadData"></param>
        /// <param name="offset"></param>
        /// <param name="requestCancellationToken"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> Patch(Uri url, byte[] uploadData, long offset, CancellationToken requestCancellationToken);

        /// <summary>
        /// tus options request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="requestCancellationToken"></param>
        /// <returns></returns>
        Task<OptionCollection> Options(Uri url, CancellationToken requestCancellationToken);
    }
}