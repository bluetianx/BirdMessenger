using System;

namespace BirdMessenger;

public class TusDeleteRequestOption:TusRequestOptionBase
{
    /// <summary>
    /// file url
    /// </summary>
    public Uri FileLocation { get; set; }
}