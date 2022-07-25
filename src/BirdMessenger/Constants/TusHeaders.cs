using System.Collections.Generic;

namespace BirdMessenger.Constants;

internal static class TusHeaders
{
    static TusHeaders()
    {
        TusReservedWords.Add("Upload-Offset".ToLower());
        TusReservedWords.Add(UploadLength.ToLower());
        TusReservedWords.Add("Upload-Length".ToLower());
        TusReservedWords.Add("Upload-Offset".ToLower());
        TusReservedWords.Add(UploadMetadata.ToLower());
        TusReservedWords.Add("Upload-Defer-Length".ToLower());
        TusReservedWords.Add("Content-Type".ToLower());
        TusReservedWords.Add("Upload-Checksum".ToLower());
        TusReservedWords.Add(TusResumable.ToLower());
        TusReservedWords.Add("Upload-Concat".ToLower());
    }
    internal static readonly HashSet<string> TusReservedWords = new();
    
    internal const string UploadLength = "Upload-Length";
    
    internal const string UploadMetadata = "Upload-Metadata";
    
    internal const string TusResumable = "Tus-Resumable";
}