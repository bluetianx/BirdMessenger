using System;
using BirdMessenger.Abstractions;

namespace BirdMessenger;

/// <summary>
/// 
/// </summary>
public sealed class TusCreateResponse:TusResponseBase
{
    /// <summary>
    /// file url
    /// </summary>
    public Uri FileLocation { get; set; }
}