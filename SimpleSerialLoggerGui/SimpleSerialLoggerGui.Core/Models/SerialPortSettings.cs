namespace SimpleSerialLoggerGui.Core.Models;

/// <summary>
/// Holds full settings for working with a com port
/// </summary>
public class SerialPortSettings
{
    /// <summary>
    /// COM1, COM2, COM17, etc... Should match com port name in Windows Device Manager exactly
    /// </summary>
    public string ComPortName { get; set; } = "";
    
    /// <summary>
    /// 9600, 115200, etc... MIN: 50, MAX: 921600
    /// </summary>
    public int BaudRate { get; set; }
    
    /// <summary>
    /// Valid options are: None, Even, Odd, Mark, Space
    /// </summary>
    public string ParityOption { get; set; } = "";
    
    /// <summary>
    /// Data bits, valid values: 6 to 9 inclusive
    /// </summary>
    public int DataBits { get; set; }

    /// <summary>
    /// Stop bits, valid values: 0, 1, 2, 1.5
    /// </summary>
    public string StopBits { get; set; } = "";
}