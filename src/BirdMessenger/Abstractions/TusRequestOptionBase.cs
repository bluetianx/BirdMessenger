using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BirdMessenger.Constants;
using BirdMessenger.Delegates;
using BirdMessenger.Infrastructure;

namespace BirdMessenger;

public abstract class TusRequestOptionBase
{
    public TusRequestOptionBase()
    {
        HttpHeaders = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// invoke before sending http request to server
    /// </summary>
    public Func<PreSendRequestEvent,Task>? OnPreSendRequestAsync { get; set; }
    
    /// <summary>
    /// add additional http headers
    /// </summary>
    public Dictionary<string,string> HttpHeaders { get; }

    /// <summary>
    /// Gets or sets a value that indicates whether upload file with streaming,upload file with streaming is efficient, default value is false,
    /// </summary>
    public bool UploadWithStreaming { get; set; }
        

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    internal void Validate()
    {
        if (HttpHeaders is not null)
        {
            foreach (var headerKey in HttpHeaders.Keys)
            {
                if (TusHeaders.TusReservedWords.Contains(headerKey.ToLower()))
                {
                    throw new TusException($"HttpHeader can not contain tus Reserved word:{headerKey}");
                }
            }
        }
    }

}