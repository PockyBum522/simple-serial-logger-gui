using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Serilog;
using SimpleSerialLoggerGui.Core.Models;

namespace SimpleSerialLoggerGui.Core.Logic.SerialLoggerHelper;

/// <summary>
/// Methods for validating/getting information about serial ports easily
/// </summary>
public class SerialPortHelpers
{
    private readonly ILogger _logger;
    private readonly SerialPortSettingsValidator _serialPortSettingsValidator;
    private readonly SerialLogger _serialLogger;
    private SerialPort? _currentSerialPort;
    private LogFormatting? _currentLogFormatting;
    
    private DateTimeOffset _lastWarningTime = DateTimeOffset.MinValue;
    private UTF8Encoding? _utf8Encoding;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    public SerialPortHelpers(ILogger logger, SerialPortSettingsValidator serialPortSettingsValidator, SerialLogger serialLogger)
    {
        
        _logger = logger;
        _serialPortSettingsValidator = serialPortSettingsValidator;
        _serialLogger = serialLogger;
    }

    /// <summary>
    /// Opens a com port and prepares for reading data from it
    /// </summary>
    /// <param name="serialPortSettings">All parameters necessary to open the serial port</param>
    /// <exception cref="COMException">Thrown if any of the parameters are invalid</exception>
    public void OpenComPort(SerialPortSettings serialPortSettings)
    {
        if (!_serialPortSettingsValidator.ComSettingsValid(serialPortSettings)) throw new ArgumentException();
        
        if (_currentSerialPort is null ||
            !_currentSerialPort.IsOpen)
        {
            _currentSerialPort = new SerialPort(
                serialPortSettings.ComPortName,
                serialPortSettings.BaudRate,
                ConvertParityToEnum(serialPortSettings.ParityOption),
                serialPortSettings.DataBits,
                ConvertStopBitsToEnum(serialPortSettings.StopBits));
                
            _utf8Encoding ??= new UTF8Encoding();

            _currentSerialPort.Encoding = _utf8Encoding;
            
            _currentSerialPort.Open();    
        }
        else
        {
            MessageBox.Show("Port already open. Was already logging.");
        }
    }    
    
    /// <summary>
    /// Starts a new serial log to file and passed TextBox 
    /// </summary>
    /// <param name="fullPathToLogfile">Full path to logfile</param>
    public void StartLogging(string fullPathToLogfile, LogFormatting logFormatSettings)
    {
        _serialLogger.StartLoggingBacker(fullPathToLogfile, logFormatSettings, _currentSerialPort);
    }
    
    
    /// <summary>
    /// Starts a new serial log to file and passed TextBox 
    /// </summary>
    /// <param name="fullPathToLogfile">Full path to logfile</param>
    public void PrepareToSendResponseImmediately(string fullPathToLogfile, LogFormatting logFormatSettings)
    {
        _serialLogger.SendResponseImmediatelyBacker(fullPathToLogfile, logFormatSettings, _currentSerialPort);
    }
    
    /// <summary>
    /// Opens com port if not already open and writes data to it
    /// </summary>
    public void WriteToSerial(SerialPortSettings serialPortSettings, string sendToSerialData)
    {
        if (_currentSerialPort is null ||
            !_currentSerialPort.IsOpen)
        {
            OpenComPort(serialPortSettings);    
        }
        
        // We're assuming it's DEC for now
        if (!BytesTextIsValid(sendToSerialData))
        {
            MessageBox.Show("You must separate multiple bytes with ',' or ' ' or both.");
            return;
        } 
        
        var splitBytes = ParseBytesText(sendToSerialData);

        if (_currentSerialPort is null) throw new NullReferenceException();
        
        _logger.Debug("Writing byte to {PortName}: {ByteToSend}", _currentSerialPort.PortName, splitBytes);

        var serialSendBuffer = ConvertBytesStringsToByteArray(splitBytes);
        
        _currentSerialPort.Write(serialSendBuffer, 0, splitBytes.Length);
    }

    private static byte[] ConvertBytesStringsToByteArray(IReadOnlyList<string> splitBytes)
    {
        var serialSendBuffer = new byte[splitBytes.Count];

        for (var i = 0; i < splitBytes.Count; i++)
        {
            var byteToSend = byte.Parse(splitBytes[i]);

            serialSendBuffer[i] = byteToSend;
        }

        return serialSendBuffer;
    }

    private bool BytesTextIsValid(string sendToSerialData)
    {
        var hasCommas = sendToSerialData.Contains(',');
        
        var hasSpaces = sendToSerialData.Contains(' ');

        if (hasCommas && !hasSpaces)
            return true;
        
        if (!hasCommas && hasSpaces)
            return true;
        
        if (hasCommas && hasSpaces)
            return true;

        if (!hasCommas && 
            !hasSpaces &&
            sendToSerialData.Length <= 3)
        {
            return true;
        }

        return false;
    }

    private string[] ParseBytesText(string sendToSerialData)
    {
        var hasCommas = sendToSerialData.Contains(',');
        
        var hasSpaces = sendToSerialData.Contains(' ');
        
        if (hasCommas && !hasSpaces)
            return sendToSerialData.Split(",");
        
        if (!hasCommas && hasSpaces)
            return sendToSerialData.Split(" ");
        
        if (hasCommas && hasSpaces)
            return sendToSerialData.Split(", ");

        if (!hasCommas && 
            !hasSpaces &&
            sendToSerialData.Length <= 3)
        {
            return new[] { sendToSerialData };
        }

        throw new ArgumentException("Text supplied to send appears to be multiple bytes but not split on ' ' or ',' or both");
    }

    /// <summary>
    /// Returns the lowest serial port available on the system
    /// </summary>
    public string GetFirstSerialPortOnSystem()
    {
        try
        {
            var availablePorts = GetAllSerialPortsOnSystem();

            return availablePorts[0];
        }
        catch (ArgumentOutOfRangeException)
        {
            // No serial ports found on the system
            return "None";
        }

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

        if (portsAsList.Count < 1)
        {
            return new List<string>(){"None"};
        }

        return portsAsList;
    }
    
    private Parity ConvertParityToEnum(string parityOption)
    {
        var parityOptionLower = parityOption.ToLower();
        
        if (parityOptionLower == "none") return Parity.None;
        if (parityOptionLower == "even") return Parity.Even;
        if (parityOptionLower == "odd") return Parity.Odd;
        if (parityOptionLower == "mark") return Parity.Mark;
        if (parityOptionLower == "space") return Parity.Space;

        _logger.Error("Parity supplied: {ParityInput} was not valid", parityOption);
        throw new ArgumentException($"Parity supplied: {parityOption} was not valid");
    }
    
    private StopBits ConvertStopBitsToEnum(string stopBits)
    {
        if (stopBits.Equals("0")) return StopBits.None;
        if (stopBits.Equals("1")) return StopBits.One;
        if (stopBits.Equals("1.5")) return StopBits.OnePointFive;
        if (stopBits.Equals("2")) return StopBits.Two;

        _logger.Error("stopBits supplied: {ParityInput} was not valid", stopBits);
        throw new ArgumentException($"stopBits supplied: {stopBits} was not valid");
    }

    /// <summary>
    /// Closes currently open com port, if any
    /// </summary>
    public void CloseComPort()
    {
        if (_currentSerialPort is null)
        {
            _logger.Warning("Tried to close serial port but no port open/_currentSettings is null");

            return;
        }
        
        _currentSerialPort.Close();
    }
}