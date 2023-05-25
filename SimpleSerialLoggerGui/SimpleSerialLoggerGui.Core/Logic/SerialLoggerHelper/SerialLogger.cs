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
            LogUntilEndingCharacterOrNoMoreData();
        }
        catch (OperationCanceledException)
        {
            _logger.Information("OperationCanceledException, user just likely closed serial port");
        }
    }

    private void LogUntilEndingCharacterOrNoMoreData()
    {
        if (_serialSerilogLogger is null) throw new NullReferenceException();
        if (_currentSerialPort is null) throw new NullReferenceException();
        
        var incomingDataByte = new byte[1];
        var builtString = "";
        int bytesToRead;
        
        do
        {
            // Read a single character
            _currentSerialPort.Read(incomingDataByte, 0, 1);

            builtString += GetNextFormattedSection(incomingDataByte[0]);

            if (CharacterIsEndLineChar(incomingDataByte[0])) break;

            bytesToRead = _currentSerialPort.BytesToRead;
        } 
        while (bytesToRead > 0);
        
        // If we got here either bytesToRead was 0 or endLineCharacter detected
        CreateLogLineOutputPerLogFormatting(builtString);
    }

    private void CreateLogLineOutputPerLogFormatting(string builtString)
    {
        if (_currentLogFormatting is null) throw new NullReferenceException();
        if (_serialSerilogLogger is null) throw new NullReferenceException();

        builtString = builtString.TrimEnd();
        builtString = builtString.TrimEnd(',');

        _serialSerilogLogger.Information("{SerialLogLine}", builtString);
    }

    private bool CharacterIsEndLineChar(byte charToCheck)
    {
        if (_currentLogFormatting is null) throw new NullReferenceException();

        if (_currentLogFormatting.LineEndingDetectionType == LogDataLineEndingDetectionType.Uninitialized)
            throw new ArgumentException("You must select a line ending detection character");

        int valueToMatchAsEndline = 0;

        switch (_currentLogFormatting.LineEndingDetectionType)
        {
            case LogDataLineEndingDetectionType.Newline:
                valueToMatchAsEndline = '\n';
                break;
            
            case LogDataLineEndingDetectionType.DecimalValue:
                valueToMatchAsEndline = int.Parse(_currentLogFormatting.LineEndingDetectionValue);
                break;
            
            default:
                throw new NotImplementedException();
        }

        if (charToCheck == valueToMatchAsEndline) return true;

        return false;
    }

    private string GetNextFormattedSection(byte characterToWork)
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
                returnString += characterToWork.ToString("X2");
                break;
            
            case LogDataDisplayType.Decimal:
                returnString = characterToWork.ToString("D3");
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
    }
}