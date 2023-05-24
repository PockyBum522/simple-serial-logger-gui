using System.IO.Ports;
using Serilog;
using SimpleSerialLoggerGui.Core.Models;

namespace SimpleSerialLoggerGui.Core.Logic.SerialLoggerHelper;

/// <summary>
/// Validates all aspects of opening a serial port
/// </summary>
public class SerialPortSettingsValidator
{
    private readonly ILogger _logger;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Application logger</param>
    public SerialPortSettingsValidator(ILogger logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Checks a passed SerialPortSettings and make sure all settings are valid to open the serial port on this system
    /// </summary>
    /// <param name="serialPortSettings">SerialPortSettings to check</param>
    /// <returns>True if all settings in the SerialPortSettings are valid, false otherwise</returns>
    public bool ComSettingsValid(SerialPortSettings serialPortSettings)
    {
        if (!SerialPortExists(serialPortSettings.ComPortName)) return false;
        
        _logger.Information("Checking if baud rate: {BaudRate} is valid", serialPortSettings.BaudRate);
        if (serialPortSettings.BaudRate is < 50 or > 921600) return false;
        
        if (!ParityOptionIsValid(serialPortSettings.ParityOption)) return false;

        _logger.Information("Checking if data bits: {DataBits} is valid", serialPortSettings.DataBits);
        if (serialPortSettings.DataBits is not 5 and not 6 and not 7 and not 8 and not 9) return false;

        _logger.Information("Checking if stop bits: {StopBits} is valid", serialPortSettings.StopBits);
        if (serialPortSettings.StopBits is not "0" and not "1" and not "1.5" and not "2") return false;
        
        // Passed all checks:
        return true;
    }
    
    private bool SerialPortExists(string comPort)
    {
        // Verify com port is one that exists on the system
        
        var availablePorts = SerialPort.GetPortNames();
        
        var comPortNameToCheckLower = comPort.ToLower();
        
        foreach(var port in availablePorts)
        {
            _logger.Information("Checking that com port: {ComPortToCheck} exists on system. Checking against: {CurrentComPort}", comPortNameToCheckLower, port);
            
            if (port.ToLower().Equals(comPortNameToCheckLower)) return true;
        }

        _logger.Warning("Couldn't find com port: {ComToCheck} on system", comPortNameToCheckLower);
        
        return false;
    }
    
    private bool ParityOptionIsValid(string parityOption)
    {
        var parityOptionLower = parityOption.ToLower();
        
        _logger.Information("Checking if parity option: {ParityOption} is valid", parityOption);

        if (parityOptionLower == "none") return true;
        if (parityOptionLower == "even") return true;
        if (parityOptionLower == "odd") return true;
        if (parityOptionLower == "mark") return true;
        if (parityOptionLower == "space") return true;

        return false;
    }
}