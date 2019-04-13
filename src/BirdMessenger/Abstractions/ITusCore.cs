using System;
using System.Collections.Generic;

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
        /// <returns></returns>
        Dictionary<string, string> Head(Uri url);

        /// <summary>
        /// tus patch request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uploadData"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        Dictionary<string, string> Patch(Uri url, byte[] uploadData,int offset);

        /// <summary>
        /// tus options request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Dictionary<string, string> Options(Uri url);
    }
}