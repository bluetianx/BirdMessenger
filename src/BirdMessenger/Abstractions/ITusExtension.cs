using System;
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
        /// Termination upload
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Delete(Uri url, CancellationToken cancellationToken = default);
    }
}