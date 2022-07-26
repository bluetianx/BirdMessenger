using System;
using System.ComponentModel;

namespace BirdMessenger.Internal;

internal static class EnumExtension
{
    internal static string GetEnumDescription(this Enum enumValue)  
    {  
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());  
  
        var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);  
  
        return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();  
    }

    internal static TusVersion ConvertToTusVersion(this string version)
    {
        TusVersion tusVersion = TusVersion.Unknown;
        if (version == TusVersion.V1_0_0.GetEnumDescription())
        {
            tusVersion = TusVersion.V1_0_0;
        }
        return tusVersion;
    }
}