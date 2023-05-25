namespace SimpleSerialLoggerGui.Core.Models.Enums;

/// <summary>
/// To store what the user selected for how to detect line end
/// </summary>
public enum LogDataLineEndingDetectionType
{
    /// <summary>
    /// Uninitialized 
    /// </summary>
    Uninitialized,
    
    /// <summary>
    /// Detect line end when '\n' is seen
    /// </summary>
    Newline,
    
    /// <summary>
    /// Detect line end when user-supplied hex value is seen 
    /// </summary>
    HexValue,
    
    /// <summary>
    /// Detect line end when user-supplied decimal value is seen 
    /// </summary>
    DecimalValue
}