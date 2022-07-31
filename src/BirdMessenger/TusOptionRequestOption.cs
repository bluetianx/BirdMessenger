using System;

namespace BirdMessenger;

public class TusOptionRequestOption:TusRequestOptionBase
{
    /// <summary>
    /// tus server address
    /// </summary>
    public Uri Endpoint { get; set; }
}