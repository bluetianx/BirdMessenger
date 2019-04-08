using System;
using System.Collections.Generic;

namespace BirdMessenger.Interfaces
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
        /// <param name="tusVersion"></param>
        /// <returns></returns>
        Dictionary<string, string> Head(Uri url, string tusVersion);
    }
}