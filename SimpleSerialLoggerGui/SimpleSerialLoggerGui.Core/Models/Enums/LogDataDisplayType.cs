namespace SimpleSerialLoggerGui.Core.Models.Enums;

/// <summary>
/// What basic type to display the logged bytes as
/// </summary>
public enum LogDataDisplayType
{
    /// <summary>
    /// Uninitialized 
    /// </summary>
    Uninitialized,
    
    /// <summary>
    /// Display bytes as ASCII characters 
    /// </summary>
    Ascii,
    
    /// <summary>
    /// Show bytes as 0x00 format 
    /// </summary>
    Hex,
    
    /// <summary>
    /// Show bytes as 255 format 
    /// </summary>
    Decimal
}