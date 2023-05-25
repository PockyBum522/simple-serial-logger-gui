using System.Collections.Generic;

namespace SimpleSerialLoggerGui.Core.Interfaces;

/// <summary>
/// Config.net interface that will be turned into a proxy object for settings pertaining to this application
/// (not library settings)
/// </summary>
public interface ISettingsApplicationLocal
{
    /// <inheritdoc cref="ISerialPortSettingsSelections" />
    public ISerialPortSettingsSelections SerialPortSettingsSelectionsSelections { get; set; }
    
    /// <inheritdoc cref="ISerialPossibleOptionsSettings" />
    public ISerialPossibleOptionsSettings SerialPossibleOptionsSettings { get; set; }
    
    /// <inheritdoc cref="ISerialLogSettings" />
    public ISerialLogSettings SerialLogSettings { get; set; }
    
    /// <inheritdoc cref="IApplicationSettings" />
    public IApplicationSettings ApplicationSettings { get; set; }
    
    /// <inheritdoc cref="LineEndingDetectionUserSelections" />
    public ILineEndingSelectionSettings LineEndingDetectionUserSelections  { get; set; }
    
    /// <inheritdoc cref="ILogByteDisplaySelectionSettings" />
    public ILogByteDisplaySelectionSettings SerialDataDisplayUserSelections { get; set; }
}

/// <summary>
/// Settings related to serial port configuration
/// </summary>
public interface ISerialPortSettingsSelections
{
    /// <summary>
    /// Last com port the user had selected
    /// </summary>
    string LastComPort { get; set; }
    
    /// <summary>
    /// Last baud rate the user had selected
    /// </summary>
    string LastBaud { get; set; }
    
    /// <summary>
    /// Last parity setting the user had selected
    /// </summary>
    string LastParity { get; set; }
    
    /// <summary>
    /// Last data bit setting the user had selected
    /// </summary>
    string LastDataBits { get; set; }
    
    /// <summary>
    /// Last stop bit setting the user had selected
    /// </summary>
    string LastStopBits { get; set; }
}

/// <summary>
/// Settings related to "Log bytes as" user selections
/// </summary>
public interface ILogByteDisplaySelectionSettings
{
    /// <summary>
    /// Whether the user had "Display as ASCII" checkbox checked
    /// </summary>
    bool LastDisplayAsAsciiState { get; set; }
    
    /// <summary>
    /// Whether the user had "Display as HEX" checkbox checked
    /// </summary>
    bool LastDisplayAsHexState { get; set; }
    
    /// <summary>
    /// Whether the user had "Display as DEC" checkbox checked
    /// </summary>
    bool LastDisplayAsDecimalState { get; set; }
    
    /// <summary>
    /// Whether the user had "Display with spaces" checkbox checked
    /// </summary>
    bool LastDisplayWithSpacesState { get; set; }
    
    /// <summary>
    /// Whether the user had "Display with CSVs" checkbox checked
    /// </summary>
    bool LastDisplayWithCommasState { get; set; }
    
    /// <summary>
    /// Whether the user had "Display with CSVs" checkbox checked
    /// </summary>
    bool LastDisplayWithNewlineCharactersState { get; set; }
}

/// <summary>
/// Settings related to "Line ending detections" user selections
/// </summary>
public interface ILineEndingSelectionSettings
{
    /// <summary>
    /// Whether the user had '\n' checkbox checked
    /// </summary>
    bool LastDetectNewlinesState { get; set; }
    
    /// <summary>
    /// Whether the user had '\r' checkbox checked
    /// </summary>
    bool LastDetectCarriageReturnsState { get; set; }
    
    /// <summary>
    /// Whether the user had '\r\n' checkbox checked
    /// </summary>
    bool LastDetectNewlineAndCarriageReturnsState { get; set; }
    
    /// <summary>
    /// Whether the user had 'Custom Hex Value' checkbox checked
    /// </summary>
    bool LastDetectHexValueChecked { get; set; }
    
    /// <summary>
    /// Whether the user had 'Custom Decimal Value' checkbox checked
    /// </summary>
    bool LastDetectDecimalValueChecked { get; set; }
    
    /// <summary>
    /// Whether the user had 'Custom Millis Value' checkbox checked
    /// </summary>
    bool LastDetectMillisValueChecked { get; set; }
    
    /// <summary>
    /// What the user had for a custom hex value
    /// </summary>
    string LastHexCustomTextState { get; set; }
    
    /// <summary>
    /// What the user had for a custom decimal value
    /// </summary>
    string LastDecimalCustomTextState { get; set; }
    
    /// <summary>
    /// What the user had for a custom millis value
    /// </summary>
    string LastMillisCustomTextState { get; set; }
}

/// <summary>
/// Settings related to serial log/log location
/// </summary>
public interface ISerialLogSettings
{
    /// <summary>
    /// Last directory to save serial log data to that the user had selected
    /// </summary>
    public string LastDirectory { get; set; }
}

/// <summary>
/// Settings related to serial port configuration
/// </summary>
public interface ISerialPossibleOptionsSettings
{
    /// <summary>
    /// Available baud rates
    /// </summary>
    string BaudRates { get; set; }
    
    /// <summary>
    /// Available data bits options
    /// </summary>
    string DataBits { get; set; }
    
    /// <summary>
    /// Available stop bits options
    /// </summary>
    string StopBits { get; set; }

    /// <summary>
    /// Available parity bit options
    /// </summary>
    string ParityOptions { get; set; }
}

/// <summary>
/// Settings related to the application in general
/// </summary>
public interface IApplicationSettings
{
    /// <summary>
    /// Whether the main window should be shown when the application starts up
    /// </summary>
    bool ShowMainWindowOnStartup { get; set; }
}
