using System;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using Serilog;
using SimpleSerialLoggerGui.Core.Models;

namespace SimpleSerialLoggerGui.Core.Logic.SerialLoggerHelper;

/// <summary>
/// For working with com ports and logging incoming data on them
/// </summary>
public class SerialLogger
{
    private readonly ILogger _logger;
    private readonly SerialPortSettingsValidator _serialPortSettingsValidator;

    private SerialPort? _currentSerialPort;
    private ILogger? _serialSerilogLogger;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected application logger</param>
    /// <param name="serialPortSettingsValidator">Injected serial port settings validator</param>
    public SerialLogger(ILogger logger, SerialPortSettingsValidator serialPortSettingsValidator)
    {
        _logger = logger;
        _serialPortSettingsValidator = serialPortSettingsValidator;
    }
    
    /// <summary>
    /// Opens a com port and prepares for reading data from it
    /// </summary>
    /// <param name="serialPortSettings">All parameters necessary to open the serial port</param>
    /// <exception cref="COMException">Thrown if any of the parameters are invalid</exception>
    public void OpenComPort(SerialPortSettings serialPortSettings)
    {
        if (!_serialPortSettingsValidator.ComSettingsValid(serialPortSettings)) throw new ArgumentException();

        _currentSerialPort = new SerialPort(
            serialPortSettings.ComPortName,
            serialPortSettings.BaudRate,
            ConvertParityToEnum(serialPortSettings.ParityOption),
            serialPortSettings.DataBits,
            ConvertStopBitsToEnum(serialPortSettings.StopBits));

        _currentSerialPort.Open();
    }

    /// <summary>
    /// Starts a new serial log to file and passed TextBox
    /// </summary>
    /// <param name="fullPathToLogfile">Full path to logfile</param>
    public void StartLogging(string fullPathToLogfile)
    {
        // Make new logger/logfile
        _serialSerilogLogger = new LoggerConfiguration()
            .Enrich.WithProperty("SerialLogger", "SerialLoggerContext")
            .MinimumLevel.Debug()
            .WriteTo.File(fullPathToLogfile, rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .WriteTo.Debug()
            .CreateLogger();

        // Any new data from serial port gets logged
        if (_currentSerialPort is null)
            throw new NullReferenceException();
        
        _currentSerialPort.DataReceived += SerialPortDataReceived;
    }

    private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (_serialSerilogLogger is null) return;

        if (_currentSerialPort is null) return;

        var incomingData = _currentSerialPort.ReadLine();
        
        _serialSerilogLogger.Information( "{IncomingData}", incomingData);
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
        
        _serialSerilogLogger = null;

        _currentSerialPort = null;
    }
}