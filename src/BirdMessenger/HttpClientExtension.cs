using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BirdMessenger.Constants;
using BirdMessenger.Delegates;
using BirdMessenger.Events;
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
            OriginHttpRequestMessage = httpReqMsg,
            OriginResponseMessage = response,
            TusResumableVersion = tusVersion
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
            throw new ArgumentNullException(nameof(reqOption));
        }

        if (reqOption.FileLocation is null)
        {
            throw new ArgumentNullException(nameof(reqOption.FileLocation));
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
            OriginHttpRequestMessage = httpReqMsg,
            OriginResponseMessage = response,
            TusResumableVersion = tusVersion,
            UploadOffset = uploadOffset,
            UploadLength = uploadLength
        };

        return tusResp;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="reqOption"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private static async Task<TusPatchResponse> TusPatchWithChunkAsync(HttpClient httpClient,TusPatchRequestOption reqOption, CancellationToken ct = default)
    {
        long totalSize = reqOption.Stream.Length;
        long uploadedSize = 0;
        TusPatchResponse tusPatchResponse = new TusPatchResponse();
        HttpRequestMessage httpReqMsg = null;
        HttpResponseMessage response = null;
        try
        {
            var tusHeadRequestOption = new TusHeadRequestOption
            {
                FileLocation = reqOption.FileLocation,
                OnPreSendRequestAsync = reqOption.OnPreSendRequestAsync
            };
            var tusHeadResp =await httpClient.TusHeadAsync(tusHeadRequestOption, ct);
            uploadedSize = tusHeadResp.UploadOffset;
            if (uploadedSize != reqOption.Stream.Position)
            {
                reqOption.Stream.Seek(uploadedSize, SeekOrigin.Begin);
            }
            var buffer = new byte[reqOption.UploadBufferSize];
            while (!ct.IsCancellationRequested)
            {
                if (totalSize == uploadedSize)
                {
                    break;
                }

                var bytesReadCount =await reqOption.Stream.ReadAsync(buffer, 0, buffer.Length, ct);
                httpReqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), reqOption.FileLocation);
                httpReqMsg.Headers.Add(TusHeaders.TusResumable,reqOption.TusVersion.GetEnumDescription());
                if (tusHeadResp.UploadLength < 0)
                {
                    httpReqMsg.Headers.Add(TusHeaders.UploadLength, totalSize.ToString());
                }
                httpReqMsg.Headers.Add(TusHeaders.UploadOffset, uploadedSize.ToString());
                reqOption.AddCustomHttpHeaders(httpReqMsg);
                httpReqMsg.Content = new ByteArrayContent(buffer,0,bytesReadCount);
                httpReqMsg.Content.Headers.Add(TusHeaders.ContentType, TusHeaders.UploadContentTypeValue);
                
                if (reqOption.OnPreSendRequestAsync is not null)
                {
                    PreSendRequestEvent preSendRequestEvent = new PreSendRequestEvent(reqOption, httpReqMsg);
                    await reqOption.OnPreSendRequestAsync(preSendRequestEvent);
                }
            
                tusPatchResponse.OriginHttpRequestMessage = httpReqMsg;
            
                response = await httpClient.SendAsync(httpReqMsg, ct);
                response.EnsureSuccessStatusCode();
            
                var tusVersion = response.GetValueOfHeader(TusHeaders.TusResumable).ConvertToTusVersion();
                uploadedSize = long.Parse(response.GetValueOfHeader(TusHeaders.UploadOffset));
            
                tusPatchResponse.TusResumableVersion = tusVersion;
                
                if (reqOption.OnProgressAsync is not null)
                {
                    UploadProgressEvent uploadProgressEvent = new UploadProgressEvent(reqOption, totalSize)
                    {
                        UploadedSize = uploadedSize
                    };
                    await reqOption.OnProgressAsync(uploadProgressEvent);
                }
            }
            if (totalSize == uploadedSize)
            {
                if (reqOption.OnCompletedAsync is not null)
                {
                    UploadCompletedEvent uploadCompletedEvent = new UploadCompletedEvent(reqOption, response);
                    await reqOption.OnCompletedAsync(uploadCompletedEvent);
                }
            }
        }
        catch (Exception e)
        {
            if (reqOption.OnFailedAsync is not null)
            {
                UploadExceptionEvent uploadExceptionEvent = new UploadExceptionEvent(reqOption, e)
                {
                    OriginHttpRequestMessage = httpReqMsg,
                    OriginResponseMessage = response
                };
                await reqOption.OnFailedAsync(uploadExceptionEvent);
            }
        }
        tusPatchResponse.OriginHttpRequestMessage = httpReqMsg;
        tusPatchResponse.OriginResponseMessage = response;
        tusPatchResponse.UploadedSize = uploadedSize;

        return tusPatchResponse;
    }

    /// <summary>
    /// resume upload file
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="reqOption"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<TusPatchResponse> TusPatchAsync(this HttpClient httpClient,TusPatchRequestOption reqOption, CancellationToken ct = default)
    {
        if (reqOption is null)
        {
            throw new ArgumentNullException(nameof(reqOption));
        }

        if (reqOption.FileLocation is null)
        {
            throw new ArgumentNullException(nameof(reqOption.FileLocation));
        }

        if (reqOption.Stream is null)
        {
            throw new ArgumentNullException(nameof(reqOption.Stream));
        }

        if (reqOption.UploadBufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(reqOption.UploadBufferSize),
                "UploadBufferSize equal or less than zero");
        }

        TusPatchResponse tusPatchResponse;
        if (reqOption.UploadOption == UploadOption.Chunk)
        {
            tusPatchResponse = await TusPatchWithChunkAsync(httpClient, reqOption, ct);
        }
        else if (reqOption.UploadOption == UploadOption.Stream)
        {
            tusPatchResponse = await TusPatchWithStreamingAsync(httpClient, reqOption, ct);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(reqOption.UploadOption));
        }

        return tusPatchResponse;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="reqOption"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private static async Task<TusPatchResponse> TusPatchWithStreamingAsync(HttpClient httpClient, TusPatchRequestOption reqOption,
        CancellationToken ct)
    {
        long totalSize = reqOption.Stream.Length;
        long uploadedSize = 0;
        TusPatchResponse tusPatchResponse = new TusPatchResponse();
        HttpRequestMessage httpReqMsg = null;
        HttpResponseMessage response = null;
        try
        {
            var tusHeadRequestOption = new TusHeadRequestOption
            {
                FileLocation = reqOption.FileLocation,
                OnPreSendRequestAsync = reqOption.OnPreSendRequestAsync
            };
            var tusHeadResp = await httpClient.TusHeadAsync(tusHeadRequestOption, ct);
            uploadedSize = tusHeadResp.UploadOffset;

            if (uploadedSize != reqOption.Stream.Position)
            {
                reqOption.Stream.Seek(uploadedSize, SeekOrigin.Begin);
            }

            UploadProgressEvent uploadProgressEvent = new UploadProgressEvent(reqOption, totalSize)
            {
                UploadedSize = uploadedSize
            };

            httpReqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), reqOption.FileLocation);
            httpReqMsg.Headers.Add(TusHeaders.TusResumable, reqOption.TusVersion.GetEnumDescription());
            if (tusHeadResp.UploadLength < 0)
            {
                httpReqMsg.Headers.Add(TusHeaders.UploadLength, totalSize.ToString());
            }

            httpReqMsg.Headers.Add(TusHeaders.UploadOffset, reqOption.Stream.Position.ToString());
            reqOption.AddCustomHttpHeaders(httpReqMsg);
            httpReqMsg.Content =
                new ProgressableStreamContent(reqOption.Stream, reqOption.UploadBufferSize, OnUploadProgress);
            httpReqMsg.Content.Headers.Add(TusHeaders.ContentType, TusHeaders.UploadContentTypeValue);
            if (reqOption.OnPreSendRequestAsync is not null)
            {
                PreSendRequestEvent preSendRequestEvent = new PreSendRequestEvent(reqOption, httpReqMsg);
                await reqOption.OnPreSendRequestAsync(preSendRequestEvent);
            }

            tusPatchResponse.OriginHttpRequestMessage = httpReqMsg;

            response = await httpClient.SendAsync(httpReqMsg, ct);
            response.EnsureSuccessStatusCode();

            var tusVersion = response.GetValueOfHeader(TusHeaders.TusResumable).ConvertToTusVersion();
            uploadedSize = long.Parse(response.GetValueOfHeader(TusHeaders.UploadOffset));

            tusPatchResponse.TusResumableVersion = tusVersion;

            async Task OnUploadProgress(long offset)
            {
                uploadedSize = offset;
                uploadProgressEvent.UploadedSize = uploadedSize;
                if (reqOption.OnProgressAsync is not null)
                {
                    await reqOption.OnProgressAsync(uploadProgressEvent);
                }
            }

            if (totalSize == uploadedSize)
            {
                if (reqOption.OnCompletedAsync is not null)
                {
                    UploadCompletedEvent uploadCompletedEvent = new UploadCompletedEvent(reqOption, response);
                    await reqOption.OnCompletedAsync(uploadCompletedEvent);
                }
            }
        }
        catch (Exception e)
        {
            if (reqOption.OnFailedAsync is not null)
            {
                UploadExceptionEvent uploadExceptionEvent = new UploadExceptionEvent(reqOption, e)
                {
                    OriginHttpRequestMessage = httpReqMsg,
                    OriginResponseMessage = response
                };
                await reqOption.OnFailedAsync(uploadExceptionEvent);
            }
        }

        tusPatchResponse.OriginHttpRequestMessage = httpReqMsg;
        tusPatchResponse.OriginResponseMessage = response;
        tusPatchResponse.UploadedSize = uploadedSize;
        return tusPatchResponse;
    }

    /// <summary>
    /// getting tusServer Info
    /// </summary>
    /// <param name="requestOption"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<TusOptionResponse> TusOptionAsync(this HttpClient httpClient,TusOptionRequestOption reqOption, CancellationToken ct)
    {
        if (reqOption is null)
        {
            throw new ArgumentNullException(nameof(reqOption));
        }

        if (reqOption.Endpoint is null)
        {
            throw new ArgumentNullException(nameof(reqOption.Endpoint));
        }
        var httpReqMsg = new HttpRequestMessage(HttpMethod.Options, reqOption.Endpoint);
        reqOption.AddCustomHttpHeaders(httpReqMsg);
        
        if (reqOption.OnPreSendRequestAsync is not null)
        {
            PreSendRequestEvent preSendRequestEvent = new PreSendRequestEvent(reqOption, httpReqMsg);
            await reqOption.OnPreSendRequestAsync(preSendRequestEvent);
        }
        var response = await httpClient.SendAsync(httpReqMsg, ct);
        response.EnsureSuccessStatusCode();
        TusOptionResponse tusOptionResponse = new TusOptionResponse();
        tusOptionResponse.OriginHttpRequestMessage = httpReqMsg;
        tusOptionResponse.OriginResponseMessage = response;
        
        var tusVersion = response.GetValueOfHeader(TusHeaders.TusResumable).ConvertToTusVersion();
        tusOptionResponse.TusResumableVersion = tusVersion;
        
        var tusVersionStr = response.GetValueOfHeader(TusHeaders.TusVersion);
        if (!string.IsNullOrWhiteSpace(tusVersionStr))
        {
            tusOptionResponse.TusVersions = tusVersionStr.Split(',').ToList();
        }

        var tusExtensionStr = response.GetValueOfHeader(TusHeaders.TusExtension);
        if (!string.IsNullOrWhiteSpace(tusExtensionStr))
        {
            tusOptionResponse.TusExtensions = tusExtensionStr.Split(',').ToList();
        }

        return tusOptionResponse;
    }


    /// <summary>
    /// delete  file
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="reqOption"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<TusDeleteResponse> TusDeleteAsync(this HttpClient httpClient, TusDeleteRequestOption reqOption, CancellationToken ct)
    {
        if (reqOption is null)
        {
            throw new ArgumentNullException(nameof(reqOption));
        }

        if (reqOption.FileLocation is null)
        {
            throw new ArgumentNullException(nameof(reqOption.FileLocation));
        }
        
        var httpReqMsg = new HttpRequestMessage(HttpMethod.Delete, reqOption.FileLocation);
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
        

        var tusResp = new TusDeleteResponse
        {
            OriginHttpRequestMessage = httpReqMsg,
            OriginResponseMessage = response,
            TusResumableVersion = tusVersion
        };

        return tusResp;
    }
}