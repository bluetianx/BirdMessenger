using System;
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
        /// <returns></returns>
        Task<Uri> Creation(Uri url, int uploadLength, string uploadMetadata);

        /// <summary>
        /// Termination upload
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task<bool> Delete(Uri url);
    }
}