using System.Collections.Generic;
using BirdMessenger.Abstractions;

namespace BirdMessenger;

public class TusOptionResponse:TusResponseBase
{
    /// <summary>
    /// server supports versions
    /// </summary>
    public List<string> TusVersions { get; set; } = new List<string>();

    /// <summary>
    /// 
    /// </summary>
    public List<string> TusExtensions { get; set; } = new List<string>();
}