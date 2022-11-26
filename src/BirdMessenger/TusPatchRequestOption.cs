using System;
using System.IO;
using System.Threading.Tasks;
using BirdMessenger.Delegates;

namespace BirdMessenger;

public class TusPatchRequestOption:TusRequestOptionBase
{
    /// <summary>
    /// file stream 
    /// </summary>
    public Stream Stream { get; set; }
    
    /// <summary>
    /// file uri
    /// </summary>
    public Uri FileLocation { get; set; }

    /// <summary>
    /// uploadSize ,default value 1MB
    /// </summary>
    public uint UploadBufferSize = 1*1024*1024;

    /// <summary>
    /// setting upload file with chunk or stream, default value is Stream
    /// </summary>
    public UploadOption UploadOption = UploadOption.Stream;
    
    /// <summary>
    /// invoke when uploading file
    /// </summary>
    public Func<UploadProgressEvent,Task>? OnProgressAsync { get; set; }
    
    /// <summary>
    /// invoke when appear a Exception
    /// </summary>
    public Func<UploadExceptionEvent,Task>? OnFailedAsync { get; set; }
    
    /// <summary>
    /// invoke when complete uploading
    /// </summary>
    public Func<UploadCompletedEvent,Task>? OnCompletedAsync { get; set; }

}