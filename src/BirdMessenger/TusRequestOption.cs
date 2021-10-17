using System.Collections.Generic;
using BirdMessenger.Infrastructure;

namespace BirdMessenger
{
    /// <summary>
    /// config http request while request tus server
    /// </summary>
    public class TusRequestOption
    {
        public TusRequestOption()
        {
            HttpHeader = new Dictionary<string, string>();
            TusReservedWords.Add("Upload-Offset".ToLower());
            TusReservedWords.Add("Content-Length".ToLower());
            TusReservedWords.Add("Upload-Length".ToLower());
            TusReservedWords.Add("Upload-Offset".ToLower());
            TusReservedWords.Add("Upload-Metadata".ToLower());
            TusReservedWords.Add("Upload-Defer-Length".ToLower());
            TusReservedWords.Add("Content-Type".ToLower());
            TusReservedWords.Add("Upload-Checksum".ToLower());
            TusReservedWords.Add("Tus-Resumable".ToLower());
            TusReservedWords.Add("Upload-Concat".ToLower());
        }
        /// <summary>
        /// add additional http header
        /// </summary>
        public Dictionary<string,string> HttpHeader { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal void Validate()
        {
            if (HttpHeader is not null)
            {
                foreach (var headerKey in HttpHeader.Keys)
                {
                    if (TusReservedWords.Contains(headerKey.ToLower()))
                    {
                        throw new TusException($"HttpHeader can not contain tus Reserved word:{headerKey}");
                    }
                }
            }
        }

        internal readonly HashSet<string> TusReservedWords = new();
    }
    
}