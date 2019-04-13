using System;

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
        Uri Creation(Uri url, int uploadLength, string uploadMetadata);

        /// <summary>
        /// Termination upload
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        bool Delete(Uri url);
    }
}