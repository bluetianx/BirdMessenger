using System;
using System.Collections.Generic;

namespace BirdMessenger;

public static class TusHeaders
{
    private static readonly HashSet<string> TusReservedWords = new(StringComparer.OrdinalIgnoreCase);

    static TusHeaders()
    {
        TusReservedWords.Add(UploadLength);
        TusReservedWords.Add(UploadOffset);
        TusReservedWords.Add(UploadMetadata);
        TusReservedWords.Add(TusResumable);
        TusReservedWords.Add(Location);
        TusReservedWords.Add(UploadDeferLength);
        TusReservedWords.Add(ContentType);
        TusReservedWords.Add(UploadChecksum);
        TusReservedWords.Add(UploadConcat);
        TusReservedWords.Add(UploadContentTypeValue);
        TusReservedWords.Add(TusVersion);
        TusReservedWords.Add(TusMaxSize);
        TusReservedWords.Add(TusExtension);
    }

    public const string UploadLength = "Upload-Length";

    public const string UploadOffset = "Upload-Offset";

    public const string UploadMetadata = "Upload-Metadata";

    public const string TusResumable = "Tus-Resumable";

    public const string Location = "Location";

    public const string UploadDeferLength = "Upload-Defer-Length";

    public const string ContentType = "Content-Type";

    public const string UploadChecksum = "Upload-Checksum";

    public const string UploadConcat = "Upload-Concat";

    public const string UploadContentTypeValue= "application/offset+octet-stream";

    public const string TusVersion = "Tus-Version";

    public const string TusMaxSize = "Tus-Max-Size";

    public const string TusExtension = "Tus-Extension";

    public static bool IsReserved(string headerName)
    {
        return string.IsNullOrWhiteSpace(headerName) is false &&
               TusReservedWords.Contains(headerName);
    }
}
