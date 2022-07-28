using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Constants;
using BirdMessenger.Delegates;
using BirdMessenger.Infrastructure;
using BirdMessenger.Internal;

namespace BirdMessenger;

public static class HttpClientExtension
{
    /// <summary>
    /// create a url for file upload
    /// </summary>
    /// <param name="reqOption"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<TusCreateResponse> TusCreateAsync(this HttpClient httpClient,TusCreateRequestOption reqOption,
        CancellationToken ct = default)
    {
        if (reqOption is null)
        {
            throw new ArgumentException("tusCreateRequestOption is null");
        }

        if (reqOption.Endpoint is null)
        {
            throw new ArgumentException("Endpoint is null");
        }

        var endpoint = reqOption.Endpoint;
        
        if (reqOption.IsUploadDeferLength && reqOption.UploadLength > 0)
        {
            throw new ArgumentException($"IsUploadDeferLength:[{reqOption.IsUploadDeferLength}] can not set true if UploadLength:[{reqOption.UploadLength}] is greater than zero");
        }
        if (!reqOption.IsUploadDeferLength && reqOption.UploadLength <= 0)
        {
            throw new ArgumentException($"IsUploadDeferLength:[{reqOption.IsUploadDeferLength}] can not set false if UploadLength:[{reqOption.UploadLength}] is less than zero");
        }
        
        var httpReqMsg = new HttpRequestMessage(HttpMethod.Post, endpoint);
        
        httpReqMsg.Headers.Add(TusHeaders.TusResumable,reqOption.TusVersion.GetEnumDescription());
        
        if (reqOption.UploadLength > 0)
        {
            httpReqMsg.Headers.Add(TusHeaders.UploadLength, reqOption.UploadLength.ToString());
        }
        else if(reqOption.IsUploadDeferLength)
        {
            httpReqMsg.Headers.Add(TusHeaders.UploadDeferLength,"1");
        }

        string uploadMetadata = reqOption.Metadata?.Serialize();
        if (!string.IsNullOrWhiteSpace(uploadMetadata))
        {
            httpReqMsg.Headers.Add(TusHeaders.UploadMetadata, uploadMetadata);
        }
        
        reqOption.AddCustomHttpHeaders(httpReqMsg);
        
        if (reqOption.OnPreSendRequestAsync is not null)
        {
            PreSendRequestEvent preSendRequestEvent = new PreSendRequestEvent(reqOption, httpReqMsg);
            await reqOption.OnPreSendRequestAsync(preSendRequestEvent);
        }
        
        var response = await httpClient.SendAsync(httpReqMsg, ct);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            throw new TusException($"creation response statusCode is {response.StatusCode}",httpReqMsg,response);
        }

        string fileUrlStr = response.GetValueOfHeader(TusHeaders.Location);
        Uri fileUrl = null;
        if (Uri.TryCreate(fileUrlStr, UriKind.RelativeOrAbsolute, out fileUrl))
        {
            if (!fileUrlStr.StartsWith("https://") && !fileUrlStr.StartsWith("http://"))
            {
                fileUrl = new Uri(endpoint, fileUrl);
            }
        }
        else
        {
            throw new TusException("Invalid location header",httpReqMsg,response);
        }

        var tusVersion = response.GetValueOfHeader(TusHeaders.TusResumable).ConvertToTusVersion();

        var tusCreateResponse = new TusCreateResponse()
        {
            FileLocation = fileUrl,
            OriginResponseMessage = response,
            TusVersion = tusVersion
        };

        return tusCreateResponse;
    }

    /// <summary>
    /// tus Head request For getting upload-offset
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="reqOption"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<TusHeadResponse> TusHeadAsync(this HttpClient httpClient,
        TusHeadRequestOption reqOption, CancellationToken ct = default)
    {
        if (reqOption is null)
        {
            throw new ArgumentException("TusHeadRequestOption is null");
        }

        if (reqOption.FileLocation is null)
        {
            throw new ArgumentException("FileLocation is null");
        }
        
        var httpReqMsg = new HttpRequestMessage(HttpMethod.Head, reqOption.FileLocation);
        httpReqMsg.Headers.Add(TusHeaders.TusResumable,reqOption.TusVersion.GetEnumDescription());
        reqOption.AddCustomHttpHeaders(httpReqMsg);
        
        if (reqOption.OnPreSendRequestAsync is not null)
        {
            PreSendRequestEvent preSendRequestEvent = new PreSendRequestEvent(reqOption, httpReqMsg);
            await reqOption.OnPreSendRequestAsync(preSendRequestEvent);
        }
        var response = await httpClient.SendAsync(httpReqMsg, ct);
        response.EnsureSuccessStatusCode();
        
        var tusVersion = response.GetValueOfHeader(TusHeaders.TusResumable).ConvertToTusVersion();
        long uploadOffset = long.Parse(response.GetValueOfHeader(TusHeaders.UploadOffset));
        if (!long.TryParse(response.GetValueOfHeaderWithoutException(TusHeaders.UploadLength), out var uploadLength))
        {
            uploadLength = -1;
        }

        var tusResp = new TusHeadResponse
        {
            OriginResponseMessage = response,
            TusVersion = tusVersion,
            UploadOffset = uploadOffset,
            UploadLength = uploadLength
        };

        return tusResp;
    }
}