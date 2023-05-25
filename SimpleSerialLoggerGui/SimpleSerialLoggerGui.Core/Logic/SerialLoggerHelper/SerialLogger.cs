using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Serilog;
using SimpleSerialLoggerGui.Core.Models;
using SimpleSerialLoggerGui.Core.Models.Enums;

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
    private LogFormatting? _currentLogFormatting;
    
    private DateTimeOffset _lastWarningTime = DateTimeOffset.MinValue;

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
    /// <param name="logFormatSettings"></param>
    /// <exception cref="COMException">Thrown if any of the parameters are invalid</exception>
    public void OpenComPort(SerialPortSettings serialPortSettings, LogFormatting logFormatSettings)
    {
        _currentLogFormatting = logFormatSettings;
        
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
        if (_currentSerialPort is null ||
            _currentLogFormatting is null)
        {
            throw new NullReferenceException();
        }

        _currentSerialPort.DataReceived += SerialPortDataReceived;
    }

    private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (_currentSerialPort is null) throw new NullReferenceException();

        try
        {
            var bytesToRead = _currentSerialPort.BytesToRead;

            // Read lines while we still have available data
            while (bytesToRead > 0 && _currentSerialPort.IsOpen)
            {
                bytesToRead = LogUntilEndingCharacterOrNoMoreData(bytesToRead);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Information("OperationCanceledException, user just likely closed serial port");
        }
    }

    private int LogUntilEndingCharacterOrNoMoreData(int bytesToRead)
    {
        if (_serialSerilogLogger is null) throw new NullReferenceException();
        if (_currentSerialPort is null) throw new NullReferenceException();
        
        var incomingDataBuffer = new char[1000];
        var currentBufferPosition = 0;

        // Read until ending char found or we run out of available data
        while (bytesToRead > 0 && _currentSerialPort.IsOpen)
        {
            _currentSerialPort.Read(incomingDataBuffer, currentBufferPosition, 1);
            currentBufferPosition++;

            // Check for line ending char(s)
            if (incomingDataBuffer.Contains('\n'))
            {
                incomingDataBuffer[currentBufferPosition] = '\0';
                break;
            }

            bytesToRead = _currentSerialPort.BytesToRead;
        }

        var bufferString = CreateLogLineOutputPerLogFormatting(incomingDataBuffer);

        if (incomingDataBuffer[0] != new char())
            _serialSerilogLogger.Information("{IncomingData}", bufferString);
        
        return bytesToRead;
    }

    private string CreateLogLineOutputPerLogFormatting(char[] incomingDataBuffer)
    {
        if (_currentLogFormatting is null) throw new NullReferenceException();

        var bufferString = "";

        foreach (var data in incomingDataBuffer)
        {
            if (data == '\0') break;

            if (!_currentLogFormatting.LogWithNewlineCharacters && data == '\r') break; 
            if (!_currentLogFormatting.LogWithNewlineCharacters && data == '\n') break; 
            
            bufferString += GetNextFormattedSection(data);
        }
        
        bufferString = bufferString.TrimEnd();
        bufferString = bufferString.TrimEnd(',');
        bufferString = bufferString.TrimEnd();

        return bufferString;
    }

    private string GetNextFormattedSection(char characterToWork)
    {
        if (_currentLogFormatting is null) throw new NullReferenceException();

        var returnString = "";
        
        switch (_currentLogFormatting.LogAsDisplayType)
        {
            case LogDataDisplayType.Ascii:
                returnString += characterToWork;
                break;
            
            case LogDataDisplayType.Hex:
                returnString = "0x";
                returnString += Convert.ToByte(characterToWork).ToString("X2");
                break;
            
            case LogDataDisplayType.Decimal:
                returnString = Convert.ToByte(characterToWork).ToString("D3");
                break;
            
            default:
                WarnUserOfInvalidLogFormatting();
                break;
        }

        if (_currentLogFormatting.LogWithCommas)
            returnString += ',';
        
        if (_currentLogFormatting.LogWithSpaces)
            returnString += ' ';
        
        return returnString;
    }

    private void WarnUserOfInvalidLogFormatting()
    {
        var secondsElapsedSinceLastWarning = _lastWarningTime - DateTimeOffset.Now;

        if (secondsElapsedSinceLastWarning.Seconds > 10)
        {
            MessageBox.Show("Invalid formatting option, please check an option for formatting and restart logging");
        } 
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