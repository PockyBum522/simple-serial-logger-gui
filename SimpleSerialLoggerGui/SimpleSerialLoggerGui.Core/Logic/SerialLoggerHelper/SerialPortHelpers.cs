using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using Serilog;

namespace SimpleSerialLoggerGui.Core.Logic.SerialLoggerHelper;

/// <summary>
/// Methods for validating/getting information about serial ports easily
/// </summary>
public class SerialPortHelpers
{
    private readonly ILogger _logger;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    public SerialPortHelpers(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Returns the lowest serial port available on the system
    /// </summary>
    public string GetFirstSerialPortOnSystem()
    {
        var availablePorts = GetAllSerialPortsOnSystem();

        return availablePorts[0];
    }
    
    /// <summary>
    /// Checks that the port with name exists on the system
    /// </summary>
    /// <param name="comPort">Name to check if exists on the system, COM1, COM2, COM17, etc...</param>
    /// <returns>True if matching port found, false otherwise</returns>
    public bool SerialPortExists(string comPort)
    {
        var availablePorts = SerialPort.GetPortNames();
        
        var comPortNameToCheckLower = comPort.ToLower();
        
        foreach(var port in availablePorts)
        {
            _logger.Information("Checking that com port: {ComPortToCheck} exists on system. Checking against: {CurrentComPort}", comPortNameToCheckLower, port);
            
            if (port.ToLower().Equals(comPortNameToCheckLower)) return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Returns all serial ports available on system as List of string
    /// </summary>
    /// <returns>List of string with all serial ports available currently</returns>
    public List<string> GetAllSerialPortsOnSystem()
    {
        var portsAsList = SerialPort.GetPortNames().ToList();

        portsAsList.Sort();

        return portsAsList;
    }

}