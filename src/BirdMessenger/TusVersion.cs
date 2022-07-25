using System.ComponentModel;

namespace BirdMessenger;

public enum TusVersion
{
    Unknown = 0,
    
    /// <summary>
    /// 1.0.0version
    /// </summary>
    [Description("1.0.0")]
    V1_0_0 = 1,
}