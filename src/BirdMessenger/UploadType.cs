using System.ComponentModel;

namespace BirdMessenger;

/// <summary>
/// tus upload file option
/// </summary>
public enum UploadType
{
    Unknown = 0,
    
    /// <summary>
    /// upload file with chunk 
    /// </summary>
    [Description("Chunk")]
    Chunk = 1,
    
    /// <summary>
    /// upload file with streaming
    /// </summary>
    [Description("Stream")]
    Stream = 2,
}