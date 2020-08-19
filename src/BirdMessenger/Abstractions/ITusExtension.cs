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
        string HttpClientName { get; set; }

        /// <summary>
        /// creation 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uploadLength"></param>
        /// <param name="uploadMetadata"></param>
        /// <param name="requestCancellationToken"></param>
        /// <returns></returns>
        Task<Uri> Creation(Uri url, long uploadLength, string uploadMetadata,CancellationToken requestCancellationToken);

        /// <summary>
        /// Termination upload
        /// </summary>
        /// <param name="url"></param>
        /// <param name="requestCancellationToken"></param>
        /// <returns></returns>
        Task<bool> Delete(Uri url,CancellationToken requestCancellationToken);
    }
}