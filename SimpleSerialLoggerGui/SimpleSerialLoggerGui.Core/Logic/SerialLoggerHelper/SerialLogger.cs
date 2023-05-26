using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Serilog;
using Serilog.Core;
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
    private LogFormatting? _currentLogFormatting;
    private SerialPort _currentSerialPort;
    
    private DateTimeOffset _lastWarningTime = DateTimeOffset.MinValue;
    private Logger _serialSerilogLogger;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected application logger</param>
    public SerialLogger(ILogger logger,
        SerialPortSettingsValidator serialPortSettingsValidator)
    {
        _logger = logger;
        
        _serialPortSettingsValidator = serialPortSettingsValidator;
    }
    
    /// <summary>
    /// Starts a new serial log to file and passed TextBox 
    /// </summary>
    /// <param name="fullPathToLogfile">Full path to logfile</param>
    public void StartLoggingBacker(string fullPathToLogfile, LogFormatting logFormatSettings, SerialPort currentSerialPort)
    {
        _currentLogFormatting = logFormatSettings;
        _currentSerialPort = currentSerialPort;

        var logFilename = Path.GetFileName(fullPathToLogfile);
        var logDirectory = Path.GetDirectoryName(fullPathToLogfile);

        var fileSafeTimestamp = DateTimeOffset.Now.ToString("s").Replace(":", "_");

        var guidString = Guid.NewGuid().ToString();
        var shortUid = guidString.Substring(2, 6);
        
        var finalLogPath = Path.Join(
            logDirectory, 
            $"{currentSerialPort.PortName}_SES_START_{fileSafeTimestamp}_{shortUid}",
            logFilename);
        
        // Make new logger/logfile
        _serialSerilogLogger = new LoggerConfiguration()
            .Enrich.WithProperty("SerialLogger", "SerialLoggerContext")
            .MinimumLevel.Debug()
            .WriteTo.File(finalLogPath, rollingInterval: RollingInterval.Day)
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
}