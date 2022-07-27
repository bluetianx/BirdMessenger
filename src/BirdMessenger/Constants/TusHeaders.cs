using System.Collections.Generic;

namespace BirdMessenger.Constants;

internal static class TusHeaders
{
    static TusHeaders()
    {
        TusReservedWords.Add(UploadLength.ToLower());
        TusReservedWords.Add(UploadOffset.ToLower());
        TusReservedWords.Add(UploadMetadata.ToLower());
        TusReservedWords.Add("Upload-Defer-Length".ToLower());
        TusReservedWords.Add("Content-Type".ToLower());
        TusReservedWords.Add("Upload-Checksum".ToLower());
        TusReservedWords.Add(TusResumable.ToLower());
        TusReservedWords.Add("Upload-Concat".ToLower());
    }
    internal static readonly HashSet<string> TusReservedWords = new();
    
    internal const string UploadLength = "Upload-Length";

    internal const string UploadOffset = "Upload-Offset";
    
    internal const string UploadMetadata = "Upload-Metadata";
    
    internal const string TusResumable = "Tus-Resumable";

    internal const string Location = "Location";
    
    
}