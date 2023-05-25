using System.Collections.Generic;

namespace SimpleSerialLoggerGui.Core.Interfaces;

/// <summary>
/// Config.net interface that will be turned into a proxy object for settings pertaining to this application
/// (not library settings)
/// </summary>
public interface ISettingsApplicationLocal
{
    /// <inheritdoc cref="ISerialSelectionSettings" />
    public ISerialSelectionSettings SerialSelectionSettings { get; set; }
    
    /// <inheritdoc cref="ISerialPossibleOptionsSettings" />
    public ISerialPossibleOptionsSettings SerialPossibleOptionsSettings { get; set; }
    
    /// <inheritdoc cref="ISerialLogSettings" />
    public ISerialLogSettings SerialLogSettings { get; set; }
    
    /// <inheritdoc cref="IApplicationSettings" />
    IApplicationSettings ApplicationSettings { get; set; }
}

/// <summary>
/// Settings related to serial port configuration
/// </summary>
public interface ISerialSelectionSettings
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
