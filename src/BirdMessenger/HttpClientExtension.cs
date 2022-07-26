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
    /// <param name="tusCreateRequestOption"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<TusCreateResponse> TusCreateAsync(this HttpClient httpClient,TusCreateRequestOption tusCreateRequestOption,
        CancellationToken ct = default)
    {
        if (tusCreateRequestOption is null)
        {
            throw new ArgumentException("tusCreateRequestOption is null");
        }

        if (tusCreateRequestOption.Endpoint is null)
        {
            throw new ArgumentException("Endpoint is null");
        }
        tusCreateRequestOption.ValidateHttpHeaders();

        var endpoint = tusCreateRequestOption.Endpoint;
        
        if (tusCreateRequestOption.IsUploadDeferLength && tusCreateRequestOption.UploadLength > 0)
        {
            throw new InvalidDataException("IsUploadDeferLength can not set true if UploadLength is greater than zero");
        }
        var httpReqMsg = new HttpRequestMessage(HttpMethod.Post, endpoint);
        
        httpReqMsg.Headers.Add(TusHeaders.TusResumable,TusVersion.V1_0_0.GetEnumDescription());
        
        if (tusCreateRequestOption.UploadLength >= 0)
        {
            httpReqMsg.Headers.Add(TusHeaders.UploadLength, tusCreateRequestOption.UploadLength.ToString());
        }

        string uploadMetadata = tusCreateRequestOption.Metadata?.Serialize();
        if (!string.IsNullOrWhiteSpace(uploadMetadata))
        {
            httpReqMsg.Headers.Add(TusHeaders.UploadMetadata, uploadMetadata);
        }
        
        if (tusCreateRequestOption.HttpHeaders is not null && tusCreateRequestOption.HttpHeaders.Any())
        {
            foreach (var key in tusCreateRequestOption.HttpHeaders.Keys)
            {
                httpReqMsg.Headers.Add(key,tusCreateRequestOption.HttpHeaders[key]);
            }
        }

        
        if (tusCreateRequestOption.OnPreSendRequestAsync is not null)
        {
            PreSendRequestEvent preSendRequestEvent = new PreSendRequestEvent
            {
                HttpRequestMsg = httpReqMsg
            };
            await tusCreateRequestOption.OnPreSendRequestAsync(preSendRequestEvent);
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
}