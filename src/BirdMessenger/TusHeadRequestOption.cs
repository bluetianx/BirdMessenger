using System;

namespace BirdMessenger;

public class TusHeadRequestOption:TusRequestOptionBase
{
    /// <summary>
    /// file url
    /// </summary>
    public Uri FileLocation { get; set; }
}