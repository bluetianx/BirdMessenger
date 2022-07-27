using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

    internal readonly TusVersion TusVersion = TusVersion.V1_0_0;

    internal void AddCustomHttpHeaders(HttpRequestMessage httpRequestMessage)
    {
        ValidateHttpHeaders();
        
        if (HttpHeaders is not null && HttpHeaders.Any())
        {
            foreach (var key in HttpHeaders.Keys)
            {
                httpRequestMessage.Headers.Add(key,HttpHeaders[key]);
            }
        }
    }
    

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private void ValidateHttpHeaders()
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