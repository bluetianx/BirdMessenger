using System;
using BirdMessenger.Collections;

namespace BirdMessenger;

public class TusCreateRequestOption:TusRequestOptionBase
{
    /// <summary>
    /// tus server address
    /// </summary>
    public Uri Endpoint { get; set; }
    
    /// <summary>
    /// indicates the size of the entire upload in bytes
    /// </summary>
    public long UploadLength { get; set; }
    
    /// <summary>
    /// indicates that the size of the upload is not known currently and will be transferred later
    /// 
    /// </summary>
    public bool IsUploadDeferLength { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public MetadataCollection Metadata = new MetadataCollection();
    
}