using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        if (!reqOption.IsUploadDeferLength && reqOption.UploadLength < 0)
        {
            throw new ArgumentException($"IsUploadDeferLength:[{reqOption.IsUploadDeferLength}] can not set false if UploadLength:[{reqOption.UploadLength}] is less than zero");
        }

        if (reqOption.UploadLength < 0)
        {
            throw new ArgumentException($"UploadLength:[{reqOption.UploadLength}] MUST be a non-negative integer");
        }
        
        var httpReqMsg = new HttpRequestMessage(HttpMethod.Post, endpoint);
        
        httpReqMsg.Headers.Add(TusHeaders.TusResumable,reqOption.TusVersion.GetEnumDescription());

        if(reqOption.IsUploadDeferLength)
        {
            httpReqMsg.Headers.Add(TusHeaders.UploadDeferLength,"1");
        }
        else
        {
            httpReqMsg.Headers.Add(TusHeaders.UploadLength, reqOption.UploadLength.ToString());
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
        if (response.StatusCode != HttpStatusCode.OK &&
            response.StatusCode != HttpStatusCode.NoContent)
        {
            throw new TusException($" head response statusCode is{response.StatusCode.ToString()} ",httpReqMsg,response);
        }
        
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
                OnPreSendRequestAsync = reqOption.OnPreSendRequestAsync,
            };
            foreach (var header in reqOption.HttpHeaders)
            {
                tusHeadRequestOption.HttpHeaders.Add(header.Key, header.Value);
            }

            var tusHeadResp = await httpClient.TusHeadAsync(tusHeadRequestOption, ct);
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

                var bytesReadCount = await reqOption.Stream.ReadAsync(buffer, 0, buffer.Length, ct);
                httpReqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), reqOption.FileLocation);
                httpReqMsg.Headers.Add(TusHeaders.TusResumable, reqOption.TusVersion.GetEnumDescription());
                if (tusHeadResp.UploadLength < 0)
                {
                    httpReqMsg.Headers.Add(TusHeaders.UploadLength, totalSize.ToString());
                }

                httpReqMsg.Headers.Add(TusHeaders.UploadOffset, uploadedSize.ToString());
                reqOption.AddCustomHttpHeaders(httpReqMsg);
                httpReqMsg.Content = new ByteArrayContent(buffer, 0, bytesReadCount);
                httpReqMsg.Content.Headers.Add(TusHeaders.ContentType, TusHeaders.UploadContentTypeValue);

                if (reqOption.OnPreSendRequestAsync is not null)
                {
                    PreSendRequestEvent preSendRequestEvent = new PreSendRequestEvent(reqOption, httpReqMsg);
                    await reqOption.OnPreSendRequestAsync(preSendRequestEvent);
                }

                tusPatchResponse.OriginHttpRequestMessage = httpReqMsg;

                response = await httpClient.SendAsync(httpReqMsg, ct);

                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new TusException($"patch response statusCode is {response.StatusCode.ToString()}",httpReqMsg,response);
                }

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
        catch (TusException tusException)
        {
            httpReqMsg = tusException.OriginHttpRequest;
            response = tusException.OriginHttpResponse;
            if(reqOption.OnFailedAsync is not null)
            {
                UploadExceptionEvent uploadExceptionEvent = new UploadExceptionEvent(reqOption, tusException)
                {
                    OriginHttpRequestMessage = httpReqMsg,
                    OriginResponseMessage = response
                };
                await reqOption.OnFailedAsync(uploadExceptionEvent);
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
        if (reqOption.IsUploadDeferLength)
        {
            if (reqOption.UploadType == UploadType.Chunk)
            {
                tusPatchResponse = await TusPatchDeferLengthWithChunkAsync(httpClient, reqOption, ct);
            }
            else if (reqOption.UploadType == UploadType.Stream)
            {
                tusPatchResponse = await TusPatchDeferLengthWithStreamingAsync(httpClient, reqOption, ct);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(reqOption.UploadType));
            }
        }
        else
        {
            if (reqOption.UploadType == UploadType.Chunk)
            {
                tusPatchResponse = await TusPatchWithChunkAsync(httpClient, reqOption, ct);
            }
            else if (reqOption.UploadType == UploadType.Stream)
            {
                tusPatchResponse = await TusPatchWithStreamingAsync(httpClient, reqOption, ct);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(reqOption.UploadType));
            }
        }
        

        return tusPatchResponse;
    }
    
    /// <summary>
     /// resume upload file with defer length (Chunk)
     /// </summary>
     /// <param name="httpClient"></param>
     /// <param name="reqOption"></param>
     /// <param name="ct"></param>
     /// <returns></returns>
     private static async Task<TusPatchResponse> TusPatchDeferLengthWithChunkAsync(HttpClient httpClient, TusPatchRequestOption reqOption, CancellationToken ct)
    {
        // Try to get total size, but handle streams where Length is not available
        long? totalSize = null;
        try
        {
            if (reqOption.Stream.CanSeek)
            {
                totalSize = reqOption.Stream.Length;
            }
        }
        catch
        {
            // Stream doesn't support length - use deferred length
        }
        
        long uploadedSize = 0;
        TusPatchResponse tusPatchResponse = new TusPatchResponse();
        HttpRequestMessage httpReqMsg = null;
        HttpResponseMessage response = null;
        try
        {
            var tusHeadRequestOption = new TusHeadRequestOption
            {
                FileLocation = reqOption.FileLocation,
                OnPreSendRequestAsync = reqOption.OnPreSendRequestAsync,
            };
            foreach (var header in reqOption.HttpHeaders)
            {
                tusHeadRequestOption.HttpHeaders.Add(header.Key, header.Value);
            }

            var tusHeadResp = await httpClient.TusHeadAsync(tusHeadRequestOption, ct);
            uploadedSize = tusHeadResp.UploadOffset;
            if (reqOption.Stream.CanSeek && uploadedSize != reqOption.Stream.Position)
            {
                reqOption.Stream.Seek(uploadedSize, SeekOrigin.Begin);
            }

            var buffer = new byte[reqOption.UploadBufferSize];
            bool reachedEndOfStream = false;
            while (!ct.IsCancellationRequested)
            {
                // For streams with known length, check if we're done
                if (totalSize.HasValue && totalSize.Value == uploadedSize)
                {
                    break;
                }

                var bytesReadCount = await reqOption.Stream.ReadAsync(buffer, 0, buffer.Length, ct);
                
                // For streams without known length, check for end of stream
                if (bytesReadCount <= 0)
                {
                    reachedEndOfStream = true;
                    break;
                }
                
                httpReqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), reqOption.FileLocation);
                httpReqMsg.Headers.Add(TusHeaders.TusResumable, reqOption.TusVersion.GetEnumDescription());
                if (tusHeadResp.UploadLength < 0)
                {
                    // Only set Upload-Length if we know the total size
                    if (totalSize.HasValue)
                    {
                        httpReqMsg.Headers.Add(TusHeaders.UploadLength, totalSize.Value.ToString());
                    }
                }

                httpReqMsg.Headers.Add(TusHeaders.UploadOffset, uploadedSize.ToString());
                reqOption.AddCustomHttpHeaders(httpReqMsg);
                httpReqMsg.Content = new ByteArrayContent(buffer, 0, bytesReadCount);
                httpReqMsg.Content.Headers.Add(TusHeaders.ContentType, TusHeaders.UploadContentTypeValue);

                if (reqOption.OnPreSendRequestAsync is not null)
                {
                    PreSendRequestEvent preSendRequestEvent = new PreSendRequestEvent(reqOption, httpReqMsg);
                    await reqOption.OnPreSendRequestAsync(preSendRequestEvent);
                }

                tusPatchResponse.OriginHttpRequestMessage = httpReqMsg;

                response = await httpClient.SendAsync(httpReqMsg, ct);

                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new TusException($"patch response statusCode is {response.StatusCode.ToString()}",httpReqMsg,response);
                }

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

            // For deferred length uploads (when size was unknown), send final PATCH to set Upload-Length
            // According to TUS spec: "the Client MUST set the Upload-Length header in the next PATCH request, once the length is known"
            // Reference: https://github.com/tus/tus-resumable-upload-protocol/blob/main/protocol.md
            if (!totalSize.HasValue && reachedEndOfStream && tusHeadResp.UploadLength < 0)
            {
                response = await SendFinalDeferredLengthPatchAsync(httpClient, reqOption, uploadedSize, ct);
                var tusVersion = response.GetValueOfHeader(TusHeaders.TusResumable).ConvertToTusVersion();
                tusPatchResponse.TusResumableVersion = tusVersion;
            }
            
            // Check if upload is complete - for known length, check if we uploaded all bytes
            // For deferred length, check if we reached end of stream
            bool isComplete = totalSize.HasValue ? (totalSize.Value == uploadedSize) : reachedEndOfStream;
            
            if (isComplete && reqOption.OnCompletedAsync is not null)
            {
                UploadCompletedEvent uploadCompletedEvent = new UploadCompletedEvent(reqOption, response);
                await reqOption.OnCompletedAsync(uploadCompletedEvent);
            }
        }
        catch (TusException tusException)
        {
            httpReqMsg = tusException.OriginHttpRequest;
            response = tusException.OriginHttpResponse;
            if(reqOption.OnFailedAsync is not null)
            {
                UploadExceptionEvent uploadExceptionEvent = new UploadExceptionEvent(reqOption, tusException)
                {
                    OriginHttpRequestMessage = httpReqMsg,
                    OriginResponseMessage = response
                };
                await reqOption.OnFailedAsync(uploadExceptionEvent);
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
    /// resume upload file with defer length (Stream)
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="reqOption"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private static async Task<TusPatchResponse> TusPatchDeferLengthWithStreamingAsync(HttpClient httpClient, TusPatchRequestOption reqOption, CancellationToken ct)
    {
        // Try to get total size, but handle streams where Length is not available
        long? totalSize = null;
        try
        {
            if (reqOption.Stream.CanSeek)
            {
                totalSize = reqOption.Stream.Length;
            }
        }
        catch
        {
            // Stream doesn't support length - use deferred length
        }
        
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
            foreach (var header in reqOption.HttpHeaders)
            {
                tusHeadRequestOption.HttpHeaders.Add(header.Key, header.Value);
            }

            var tusHeadResp = await httpClient.TusHeadAsync(tusHeadRequestOption, ct);
            uploadedSize = tusHeadResp.UploadOffset;

            if (reqOption.Stream.CanSeek && uploadedSize != reqOption.Stream.Position)
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
                // Only set Upload-Length if we know the total size
                if (totalSize.HasValue)
                {
                    httpReqMsg.Headers.Add(TusHeaders.UploadLength, totalSize.Value.ToString());
                }
            }

            httpReqMsg.Headers.Add(TusHeaders.UploadOffset, uploadedSize.ToString());
            reqOption.AddCustomHttpHeaders(httpReqMsg);
            httpReqMsg.Content =
                new ProgressableStreamContentDeferLength(reqOption.Stream, reqOption.UploadBufferSize, OnUploadProgress);
            httpReqMsg.Content.Headers.Add(TusHeaders.ContentType, TusHeaders.UploadContentTypeValue);
            if (reqOption.OnPreSendRequestAsync is not null)
            {
                PreSendRequestEvent preSendRequestEvent = new PreSendRequestEvent(reqOption, httpReqMsg);
                await reqOption.OnPreSendRequestAsync(preSendRequestEvent);
            }

            tusPatchResponse.OriginHttpRequestMessage = httpReqMsg;

            response = await httpClient.SendAsync(httpReqMsg, ct);

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new TusException($"patch response statusCode is {response.StatusCode.ToString()}",httpReqMsg,response);
            }

            var tusVersion = response.GetValueOfHeader(TusHeaders.TusResumable).ConvertToTusVersion();
            var serverUploadedSize = long.Parse(response.GetValueOfHeader(TusHeaders.UploadOffset));

            tusPatchResponse.TusResumableVersion = tusVersion;

            async Task OnUploadProgress(long offset)
            {
                uploadProgressEvent.UploadedSize = offset;
                if (reqOption.OnProgressAsync is not null)
                {
                    await reqOption.OnProgressAsync(uploadProgressEvent);
                }
            }

            // The server responded with 204 No Content, indicating successful upload.
            // The Upload-Offset header contains the total bytes received by the server.
            uploadedSize = serverUploadedSize;
            
            // For deferred length uploads (when size was unknown), send final PATCH to set Upload-Length
            // According to TUS spec: "the Client MUST set the Upload-Length header in the next PATCH request, once the length is known"
            // Reference: https://github.com/tus/tus-resumable-upload-protocol/blob/main/protocol.md
            if (!totalSize.HasValue && tusHeadResp.UploadLength < 0)
            {
                response = await SendFinalDeferredLengthPatchAsync(httpClient, reqOption, uploadedSize, ct);
                var finalTusVersion = response.GetValueOfHeader(TusHeaders.TusResumable).ConvertToTusVersion();
                tusPatchResponse.TusResumableVersion = finalTusVersion;
            }
            
            // For streaming upload with known length, verify server received expected amount
            // For deferred-length, the server's 204 response confirms successful completion
            if (reqOption.OnCompletedAsync is not null)
            {
                bool isComplete = !totalSize.HasValue || (totalSize.Value == uploadedSize);
                
                if (isComplete)
                {
                    UploadCompletedEvent uploadCompletedEvent = new UploadCompletedEvent(reqOption, response);
                    await reqOption.OnCompletedAsync(uploadCompletedEvent);
                }
            }
        }
        catch (TusException tusException)
        {
            httpReqMsg = tusException.OriginHttpRequest;
            response = tusException.OriginHttpResponse;
            if (reqOption.OnFailedAsync is not null)
            {
                UploadExceptionEvent uploadExceptionEvent = new UploadExceptionEvent(reqOption, tusException)
                {
                    OriginHttpRequestMessage = httpReqMsg,
                    OriginResponseMessage = response
                };
                await reqOption.OnFailedAsync(uploadExceptionEvent);
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
    /// Sends a final PATCH request to set Upload-Length for deferred uploads.
    /// According to TUS spec: "the Client MUST set the Upload-Length header in the next PATCH request, once the length is known"
    /// </summary>
    private static async Task<HttpResponseMessage> SendFinalDeferredLengthPatchAsync(
        HttpClient httpClient,
        TusPatchRequestOption reqOption,
        long uploadedSize,
        CancellationToken ct)
    {
        var httpReqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), reqOption.FileLocation);
        httpReqMsg.Headers.Add(TusHeaders.TusResumable, reqOption.TusVersion.GetEnumDescription());
        httpReqMsg.Headers.Add(TusHeaders.UploadLength, uploadedSize.ToString());
        httpReqMsg.Headers.Add(TusHeaders.UploadOffset, uploadedSize.ToString());
        reqOption.AddCustomHttpHeaders(httpReqMsg);
        httpReqMsg.Content = new ByteArrayContent(Array.Empty<byte>());
        httpReqMsg.Content.Headers.Add(TusHeaders.ContentType, TusHeaders.UploadContentTypeValue);

        if (reqOption.OnPreSendRequestAsync is not null)
        {
            PreSendRequestEvent preSendRequestEvent = new PreSendRequestEvent(reqOption, httpReqMsg);
            await reqOption.OnPreSendRequestAsync(preSendRequestEvent);
        }

        var response = await httpClient.SendAsync(httpReqMsg, ct);
        
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            throw new TusException($"patch response statusCode is {response.StatusCode.ToString()}",httpReqMsg,response);
        }

        return response;
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
            foreach (var header in reqOption.HttpHeaders)
            {
                tusHeadRequestOption.HttpHeaders.Add(header.Key, header.Value);
            }

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

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new TusException($"patch response statusCode is {response.StatusCode.ToString()}",httpReqMsg,response);
            }

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
        catch (TusException tusException)
        {
            httpReqMsg = tusException.OriginHttpRequest;
            response = tusException.OriginHttpResponse;
            if (reqOption.OnFailedAsync is not null)
            {
                UploadExceptionEvent uploadExceptionEvent = new UploadExceptionEvent(reqOption, tusException)
                {
                    OriginHttpRequestMessage = httpReqMsg,
                    OriginResponseMessage = response
                };
                await reqOption.OnFailedAsync(uploadExceptionEvent);
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
        if (response.StatusCode != HttpStatusCode.OK && 
            response.StatusCode != HttpStatusCode.NoContent)
        {
            throw new TusException($"options response statusCode is {response.StatusCode}",httpReqMsg,response);
        }
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
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            throw new TusException($"delete response statusCode is {response.StatusCode}",httpReqMsg,response);
        }
        
        var tusVersion = response.GetValueOfHeader(TusHeaders.TusResumable).ConvertToTusVersion();
        

        var tusResp = new TusDeleteResponse
        {
            OriginHttpRequestMessage = httpReqMsg,
            OriginResponseMessage = response,
            TusResumableVersion = tusVersion
        };

        return tusResp;
    }

    /// <summary>
    /// download file from server
    /// </summary>
    public static async Task<TusDownloadResponse> TusDownloadAsync(this HttpClient httpClient,
        TusDownloadRequestOption reqOption, CancellationToken ct = default)
    {
        if (reqOption is null)
        {
            throw new ArgumentNullException(nameof(reqOption));
        }

        if (reqOption.FileLocation is null)
        {
            throw new ArgumentNullException(nameof(reqOption));
        }

        if (reqOption.OutputStream is null)
        {
            throw new ArgumentNullException(nameof(reqOption));
        }

        if (reqOption.DownloadBufferSize == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(reqOption.DownloadBufferSize),
                "DownloadBufferSize must be greater than zero");
        }

        long downloadedSize = 0;
        long? totalSize = null;
        HttpRequestMessage httpReqMsg = null;
        HttpResponseMessage response = null;
        var tusDownloadResponse = new TusDownloadResponse();

        try
        {
            httpReqMsg = new HttpRequestMessage(HttpMethod.Get, reqOption.FileLocation);
            httpReqMsg.Headers.Add(TusHeaders.TusResumable, reqOption.TusVersion.GetEnumDescription());

            if (reqOption.OutputStream.CanSeek && reqOption.OutputStream.Position > 0)
            {
                var offset = reqOption.OutputStream.Position;
                httpReqMsg.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(offset, null);
            }

            reqOption.AddCustomHttpHeaders(httpReqMsg);

            if (reqOption.OnPreSendRequestAsync is not null)
            {
                PreSendRequestEvent preSendRequestEvent = new PreSendRequestEvent(reqOption, httpReqMsg);
                await reqOption.OnPreSendRequestAsync(preSendRequestEvent);
            }

            response = await httpClient.SendAsync(httpReqMsg, HttpCompletionOption.ResponseHeadersRead, ct);

            if (response.StatusCode != HttpStatusCode.OK &&
                response.StatusCode != HttpStatusCode.PartialContent)
            {
                throw new TusException($"download response statusCode is {response.StatusCode}", httpReqMsg,
                    response);
            }

            var tusVersionStr = response.GetValueOfHeaderWithoutException(TusHeaders.TusResumable);
            if (!string.IsNullOrWhiteSpace(tusVersionStr))
            {
                tusDownloadResponse.TusResumableVersion = tusVersionStr.ConvertToTusVersion();
            }

            if (response.StatusCode == HttpStatusCode.PartialContent)
            {
                if (response.Content.Headers.ContentRange != null)
                {
                    totalSize = response.Content.Headers.ContentRange.Length;
                }

                downloadedSize = reqOption.OutputStream.CanSeek ? reqOption.OutputStream.Position : 0;
            }
            else
            {
                downloadedSize = 0;
            }

            if (totalSize == null && response.Content.Headers.ContentLength.HasValue)
            {
                if (response.StatusCode == HttpStatusCode.PartialContent && reqOption.OutputStream.CanSeek)
                {
                    totalSize = response.Content.Headers.ContentLength + reqOption.OutputStream.Position;
                }
                else
                {
                    totalSize = response.Content.Headers.ContentLength;
                }
            }

            var downloadProgressEvent = new DownloadProgressEvent(reqOption, totalSize)
            {
                DownloadedSize = downloadedSize
            };

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                var buffer = new byte[reqOption.DownloadBufferSize];
                int bytesRead;

                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                {
                    await reqOption.OutputStream.WriteAsync(buffer, 0, bytesRead, ct);
                    downloadedSize += bytesRead;

                    if (reqOption.OnProgressAsync is not null)
                    {
                        downloadProgressEvent.DownloadedSize = downloadedSize;
                        await reqOption.OnProgressAsync(downloadProgressEvent);
                    }
                }
            }

            tusDownloadResponse.DownloadedSize = downloadedSize;
            tusDownloadResponse.TotalSize = totalSize ?? downloadedSize;

            if (reqOption.OnCompletedAsync is not null)
            {
                DownloadCompletedEvent downloadCompletedEvent = new DownloadCompletedEvent(reqOption, response);
                await reqOption.OnCompletedAsync(downloadCompletedEvent);
            }
        }
        catch (TusException tusException)
        {
            httpReqMsg = tusException.OriginHttpRequest;
            response = tusException.OriginHttpResponse;
            if (reqOption.OnFailedAsync is not null)
            {
                DownloadExceptionEvent downloadExceptionEvent =
                    new DownloadExceptionEvent(reqOption, tusException)
                    {
                        OriginHttpRequestMessage = httpReqMsg,
                        OriginResponseMessage = response
                    };
                await reqOption.OnFailedAsync(downloadExceptionEvent);
            }
        }
        catch (Exception e)
        {
            if (reqOption.OnFailedAsync is not null)
            {
                DownloadExceptionEvent downloadExceptionEvent = new DownloadExceptionEvent(reqOption, e)
                {
                    OriginHttpRequestMessage = httpReqMsg,
                    OriginResponseMessage = response
                };
                await reqOption.OnFailedAsync(downloadExceptionEvent);
            }
        }

        tusDownloadResponse.OriginHttpRequestMessage = httpReqMsg;
        tusDownloadResponse.OriginResponseMessage = response;
        tusDownloadResponse.DownloadedSize = downloadedSize;

        return tusDownloadResponse;
    }
}