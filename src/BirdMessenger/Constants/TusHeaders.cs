using System.Collections.Generic;

namespace BirdMessenger.Constants;

internal static class TusHeaders
{
    static TusHeaders()
    {
        TusReservedWords.Add(UploadLength.ToLower());
        TusReservedWords.Add(UploadOffset.ToLower());
        TusReservedWords.Add(UploadMetadata.ToLower());
        TusReservedWords.Add(UploadDeferLength.ToLower());
        TusReservedWords.Add(ContentType.ToLower());
        TusReservedWords.Add(UploadChecksum.ToLower());
        TusReservedWords.Add(TusResumable.ToLower());
        TusReservedWords.Add(UploadConcat.ToLower());
    }
    internal static readonly HashSet<string> TusReservedWords = new();
    
    internal const string UploadLength = "Upload-Length";

    internal const string UploadOffset = "Upload-Offset";
    
    internal const string UploadMetadata = "Upload-Metadata";
    
    internal const string TusResumable = "Tus-Resumable";

    internal const string Location = "Location";
    
    internal const string UploadDeferLength = "Upload-Defer-Length";
    
    internal const string ContentType = "Content-Type";
    
    internal const string UploadChecksum = "Upload-Checksum";
    
    internal const string UploadConcat = "Upload-Concat";
    
    
}