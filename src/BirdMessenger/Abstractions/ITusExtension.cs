using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BirdMessenger.Abstractions
{
    /// <summary>
    /// tus protocol extensions
    /// </summary>
    public interface ITusExtension
    {
        /// <summary>
        /// creation 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uploadLength"></param>
        /// <param name="uploadMetadata"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Uri> Creation(Uri url, long uploadLength, string uploadMetadata, CancellationToken cancellationToken = default);

        /// <summary>
        /// create partial file
        /// </summary>
        /// <param name="host"></param>
        /// <param name="uploadLength"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Uri> CreatePartialAsync(Uri host, long uploadLength, CancellationToken ct = default);

        /// <summary>
        /// Concatenate partial files
        /// </summary>
        /// <param name="host"></param>
        /// <param name="partialFiles"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Uri> ConcatenateAsync(Uri host, string[] partialFiles, CancellationToken ct = default);
        /// <summary>
        /// Termination upload
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Delete(Uri url, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creation With Upload
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uploadLength"></param>
        /// <param name="uploadMetadata"></param>
        /// <param name="uploadData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> CreationWithUploadAsync(Uri url, long uploadLength, string uploadMetadata,byte[] uploadData, CancellationToken cancellationToken = default);
    }
}