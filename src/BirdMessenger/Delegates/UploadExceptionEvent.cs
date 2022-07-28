using System;

namespace BirdMessenger.Delegates;

public class UploadExceptionEvent:UploadEvent
{
    public UploadExceptionEvent(TusRequestOptionBase tusRequestOption,Exception exception) : base(tusRequestOption)
    {
        Exception = exception;
    }
    
    /// <summary>
    /// exception arose while uploading 
    /// </summary>
    public Exception Exception { get; }
}