using SimpleSerialLoggerGui.Core.Models.Enums;

namespace SimpleSerialLoggerGui.Core.Models;

/// <summary>
/// Model to store settings that the SerialLogger will need to format the data output
/// </summary>
public class LogFormatting
{
    /// <inheritdoc cref="LogDataDisplayType"/>
    public LogDataDisplayType LogAsDisplayType { get; set; }
    
    /// <inheritdoc cref="LogDataLineEndingDetectionType"/>
    public LogDataLineEndingDetectionType LineEndingDetectionType { get; set; }
    
    /// <summary>
    /// Should we include spaces between serial bytes
    /// </summary>
    public bool LogWithSpaces { get; set; }
    
    /// <summary>
    /// Should we include commas between serial bytes
    /// </summary>
    public bool LogWithCommas { get; set; }

    /// <summary>
    /// Should \n and \r be included in the log lines
    /// </summary>
    public bool LogWithNewlineCharacters { get; set; }

    /// <summary>
    /// What to match to consider a line from the serial port finished
    /// </summary>
    public string LineEndingDetectionValue { get; set; } = "";
}