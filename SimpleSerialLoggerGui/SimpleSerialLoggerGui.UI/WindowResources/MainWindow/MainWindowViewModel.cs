using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using SimpleSerialLoggerGui.Core;
using SimpleSerialLoggerGui.Core.Interfaces;
using SimpleSerialLoggerGui.Core.Logic.SerialLoggerHelper;
using SimpleSerialLoggerGui.Core.Logic.WindowsHelpers;
using SimpleSerialLoggerGui.Core.Models;
using SimpleSerialLoggerGui.Core.Models.Enums;

namespace SimpleSerialLoggerGui.UI.WindowResources.MainWindow;

/// <summary>
/// The ViewModel for MainWindow
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _currentSerialLog = "";

    [ObservableProperty] private string _currentLogFilename = "";
    [ObservableProperty] private string _builtLogFilename = "";
    
    [ObservableProperty] private string _pathToSaveLogsIn = "";
    [ObservableProperty] private string _logFilenameOptionalAppend = "";
    
    [ObservableProperty] private string _selectedComPort = "";
    [ObservableProperty] private string _selectedBaud = "";
    [ObservableProperty] private string _selectedParity = "";
    [ObservableProperty] private string _selectedDataBits = "";
    [ObservableProperty] private string _selectedStopBits = "";
    
    [ObservableProperty] private bool _isCheckedLogAsAscii;
    [ObservableProperty] private bool _isCheckedLogAsHex;
    [ObservableProperty] private bool _isCheckedLogAsDecimal;
    [ObservableProperty] private bool _isCheckedLogWithSpaces;
    [ObservableProperty] private bool _isCheckedLogWithCommas;
    [ObservableProperty] private bool _isCheckedLogNewlineCharacters;
    
    [ObservableProperty] private bool _isCheckedLineEndingDetectionOnNewLine;
    [ObservableProperty] private bool _isCheckedLineEndingDetectionOnHexValue;
    [ObservableProperty] private bool _isCheckedLineEndingDetectionOnDecimalValue;
    [ObservableProperty] private string _hexValueForLineEndingDetection = "";
    [ObservableProperty] private string _decimalValueForLineEndingDetection = "";
    
    [ObservableProperty] private string _sendToSerialData = "";
    
    [ObservableProperty] private bool _comPortSettingsControlsEnabled = true;
    
    [ObservableProperty] private ObservableCollection<string> _comPorts = new();
    [ObservableProperty] private ObservableCollection<string> _baudRates = new();
    [ObservableProperty] private ObservableCollection<string> _parityOptions = new();
    [ObservableProperty] private ObservableCollection<string> _dataBitOptions = new();
    [ObservableProperty] private ObservableCollection<string> _stopBitOptions = new();

    [ObservableProperty] private Brush _stopLoggingButtonBackgroundColor;
    [ObservableProperty] private Brush _stopLoggingButtonForegroundColor;
    
    [ObservableProperty] private Brush _startLoggingButtonBackgroundColor;
    [ObservableProperty] private Brush _startLoggingButtonForegroundColor;

    private string _sessionLogFolderPath = "";
    
    // Button colors to notify user what current mode is
    private Brush DarkRed => new SolidColorBrush(Color.FromArgb(255, 25, 10, 10));
    private Brush LightRed => new SolidColorBrush(Color.FromArgb(255, 135, 10, 10));
    private Brush DarkGreen => new SolidColorBrush(Color.FromArgb(255, 10, 25, 10));
    private Brush LightGreen => new SolidColorBrush(Color.FromArgb(255, 10, 135, 10));
    private Brush TextDark => new SolidColorBrush(Color.FromArgb(255, 20, 20, 20));
    private Brush TextLight => new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
    
    
    private readonly ILogger _logger;
    private readonly Dispatcher _uiThreadDispatcher;
    private readonly SerialLogger _serialLogger;
    private readonly ISettingsApplicationLocal _settingsApplicationLocal;
    private readonly SerialPortHelpers _serialPortHelpers;
    private string _lastFileData;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    /// <param name="uiThreadDispatcher">UI Thread dispatcher so we can invoke UI changes later</param>
    /// <param name="serialLogger">Injected SerialLogger</param>
    /// <param name="settingsApplicationLocal">ISettingsApplicationLocal proxy object from Config.net that was set up in DIContainerBuilder.cs</param>
    /// <param name="serialPortHelpers">Injected serial port helpers</param>
    public MainWindowViewModel(ILogger logger, 
        Dispatcher uiThreadDispatcher,
        SerialLogger serialLogger,
        ISettingsApplicationLocal settingsApplicationLocal,
        SerialPortHelpers serialPortHelpers)
    {
        _logger = logger;
        _uiThreadDispatcher = uiThreadDispatcher;
        _serialLogger = serialLogger;
        _settingsApplicationLocal = settingsApplicationLocal;
        _serialPortHelpers = serialPortHelpers;
    }

    private Task UpdateLogTextboxWithFileContents()
    {
        _lastFileData = "";

        if (string.IsNullOrWhiteSpace(_sessionLogFolderPath)) return Task.CompletedTask;
        
        if (!Directory.Exists(_sessionLogFolderPath)) return Task.CompletedTask;
        
        if (Directory.GetFiles(_sessionLogFolderPath).Length < 1) return Task.CompletedTask;
        
        var fileReader = new FileReader();

        var newestLogFilePath = FolderHelpers.GetNewestFileIn(_sessionLogFolderPath, "*.log");
        
        var fileData = fileReader.ReadFileWithoutLocking(newestLogFilePath);

        if (_lastFileData == fileData) return Task.CompletedTask;

        _lastFileData = fileData;
        
        _uiThreadDispatcher.Invoke(() =>
        {
            CurrentSerialLog = fileData;
        });
        
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void StartNewLogFile()
    {
        if (!WarnUserUnlessAllSettingsValid()) return;
        
        SaveAllUserSettingsToConfiguration();
        
        var logFormatSettings = new LogFormatting()
        {
            LogAsDisplayType = SetLogDataDisplayTypeFromUi(),
            LineEndingDetectionType = SetLogDataLineEndingDetectionTypeFromUi(),
            LineEndingDetectionValue = SetLineEndingValueFromUi(),
            LogWithCommas = IsCheckedLogWithCommas,
            LogWithSpaces = IsCheckedLogWithSpaces,
            LogWithNewlineCharacters = IsCheckedLogNewlineCharacters
        };

        var serialSettings = new SerialPortSettings()
        {
            BaudRate = int.Parse(SelectedBaud),
            ComPortName = SelectedComPort,
            DataBits = int.Parse(SelectedDataBits),
            StopBits = SelectedStopBits,
            ParityOption = SelectedParity
        };
        
        // Open com port with user selected settings
        _serialPortHelpers.OpenComPort(serialSettings);
        
        // Disable com port controls from changes allowed
        ComPortSettingsControlsEnabled = false;

        // If successful, start logging with BuiltLogFilename

        var baseLogPath = Path.Join(
            PathToSaveLogsIn,
            "serial_data_.log");
                
        var fullLogPath = GetSessionLogPath(baseLogPath, serialSettings);
        
        _sessionLogFolderPath = 
            Path.GetDirectoryName(fullLogPath) ?? "";

        Directory.CreateDirectory(_sessionLogFolderPath);
        
        _serialPortHelpers.StartLogging(fullLogPath, logFormatSettings);
        //_serialPortHelpers.PrepareToSendResponseImmediately(fullLogPath, logFormatSettings);
        
        StopLoggingButtonBackgroundColor = DarkRed;
        StopLoggingButtonForegroundColor = TextLight;
        
        StartLoggingButtonBackgroundColor = LightGreen;
        StartLoggingButtonForegroundColor = TextDark; 
        
        // TODO: Update CurrentLogFilename
    }

    private string GetSessionLogPath(string baseLogPath, SerialPortSettings serialSettings)
    {
        var logFilename = Path.GetFileName(baseLogPath);
        var logDirectory = Path.GetDirectoryName(baseLogPath);

        var fileSafeTimestamp = DateTimeOffset.Now.ToString("s").Replace(":", "_");

        var guidString = Guid.NewGuid().ToString();
        var shortUid = guidString.Substring(2, 6);
        
        var finalLogPath = Path.Join(
            logDirectory, 
            $"{serialSettings.ComPortName}_SES_START_{fileSafeTimestamp}_{shortUid}",
            logFilename);

        return finalLogPath;
    }

    [RelayCommand]
    private void RescanSystemComPorts()
    {
        ComPorts.Clear();
        
        // Get all port names on system
        foreach (var portNames in _serialPortHelpers.GetAllSerialPortsOnSystem())
        {
            ComPorts.Add(portNames.Trim());
        }
    }


    [RelayCommand]
    private void OpenLogsFolderInFileExplorer()
    {
        Process.Start("explorer.exe", PathToSaveLogsIn);
    }

    [RelayCommand]
    private void SendDataToSerial()
    {
        var serialSettings = new SerialPortSettings()
        {
            BaudRate = int.Parse(SelectedBaud),
            ComPortName = SelectedComPort,
            DataBits = int.Parse(SelectedDataBits),
            StopBits = SelectedStopBits,
            ParityOption = SelectedParity
        };
        
        _serialPortHelpers.WriteToSerial(serialSettings, SendToSerialData);
    }
    
    private bool WarnUserUnlessAllSettingsValid()
    {
        if (string.IsNullOrWhiteSpace(PathToSaveLogsIn))
        {
            MessageBox.Show("Log to directory is empty or invalid. Please set a directory to log serial data to and try again.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(SelectedComPort))
        {
            MessageBox.Show("Please pick a serial port");
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(SelectedBaud))
        {
            MessageBox.Show("Please pick a baud rate");
            return false;
        }

        if (!IsCheckedLogAsAscii &&
            !IsCheckedLogAsHex &&
            !IsCheckedLogAsDecimal)
        {
            MessageBox.Show("You must select a value to display logged bytes as");
            return false;
        }
        
        // Line ending detection
        if (!IsCheckedLineEndingDetectionOnNewLine &&
            !IsCheckedLineEndingDetectionOnDecimalValue &&
            !IsCheckedLineEndingDetectionOnHexValue)
        {
            MessageBox.Show("You must select a criteria for line endings");
            return false;
        }
        
        if (IsCheckedLineEndingDetectionOnDecimalValue &&
            string.IsNullOrWhiteSpace(DecimalValueForLineEndingDetection))
        {
            MessageBox.Show("You must enter a value to match on for line endings under decimal");
            return false;
        }
        
        if (IsCheckedLineEndingDetectionOnHexValue &&
            string.IsNullOrWhiteSpace(HexValueForLineEndingDetection))
        {
            MessageBox.Show("You must enter a value to match on for line endings under hex");
            return false;
        }

        return true;
    }

    private void SaveAllUserSettingsToConfiguration()
    {
        // Load last selected settings, if valid
        _settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastBaud = SelectedBaud;

        _settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastParity = SelectedParity;

        _settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastDataBits = SelectedDataBits;

        _settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastStopBits = SelectedStopBits;

        _settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastComPort = SelectedComPort;

        // Log bytes as display selections
        _settingsApplicationLocal.SerialDataDisplayUserSelections.LastDisplayAsAsciiState = IsCheckedLogAsAscii;

        _settingsApplicationLocal.SerialDataDisplayUserSelections.LastDisplayAsHexState = IsCheckedLogAsHex;

        _settingsApplicationLocal.SerialDataDisplayUserSelections.LastDisplayAsDecimalState = IsCheckedLogAsDecimal;

        _settingsApplicationLocal.SerialDataDisplayUserSelections.LastDisplayWithSpacesState = IsCheckedLogWithSpaces;

        _settingsApplicationLocal.SerialDataDisplayUserSelections.LastDisplayWithCommasState = IsCheckedLogWithCommas;

        _settingsApplicationLocal.SerialDataDisplayUserSelections.LastDisplayWithNewlineCharactersState = IsCheckedLogNewlineCharacters;

        // Line ending detection settings
        _settingsApplicationLocal.LineEndingDetectionUserSelections.LastDetectNewlinesState = IsCheckedLineEndingDetectionOnNewLine;

        _settingsApplicationLocal.LineEndingDetectionUserSelections.LastDetectHexValueChecked = IsCheckedLineEndingDetectionOnHexValue;

        _settingsApplicationLocal.LineEndingDetectionUserSelections.LastDetectDecimalValueChecked = IsCheckedLineEndingDetectionOnDecimalValue;

        _settingsApplicationLocal.LineEndingDetectionUserSelections.LastDecimalCustomTextState = DecimalValueForLineEndingDetection;
        
        _settingsApplicationLocal.LineEndingDetectionUserSelections.LastHexCustomTextState = HexValueForLineEndingDetection;

        // Path for logs directory
        _settingsApplicationLocal.SerialLogSettings.LastDirectory = PathToSaveLogsIn;
    }

    private string SetLineEndingValueFromUi()
    {
        if (IsCheckedLineEndingDetectionOnNewLine)
        {
            var returnString = "";
            returnString += '\n';
            return returnString;
        }

        if (IsCheckedLineEndingDetectionOnHexValue)
            throw new NotImplementedException();
        
        if (IsCheckedLineEndingDetectionOnDecimalValue)
            return DecimalValueForLineEndingDetection;

        return "";
    }

    private LogDataDisplayType SetLogDataDisplayTypeFromUi()
    {
        var displayType = LogDataDisplayType.Uninitialized;
        
        if (IsCheckedLogAsAscii)
            displayType = LogDataDisplayType.Ascii;

        if (IsCheckedLogAsHex)
            displayType = LogDataDisplayType.Hex;

        if (IsCheckedLogAsDecimal)
            displayType = LogDataDisplayType.Decimal;
        
        return displayType;
    }
    
    private LogDataLineEndingDetectionType SetLogDataLineEndingDetectionTypeFromUi()
    {
        var lineEndingDetectionType = LogDataLineEndingDetectionType.Uninitialized;

        if (IsCheckedLineEndingDetectionOnNewLine)
            lineEndingDetectionType = LogDataLineEndingDetectionType.Newline;

        if (IsCheckedLineEndingDetectionOnHexValue)
            lineEndingDetectionType = LogDataLineEndingDetectionType.HexValue;

        if (IsCheckedLineEndingDetectionOnDecimalValue)
            lineEndingDetectionType = LogDataLineEndingDetectionType.DecimalValue;

        return lineEndingDetectionType;
    }

    [RelayCommand]
    private void StopLogging()
    {
        // Close com port with user selected settings
        _serialPortHelpers.CloseComPort();
        
        // Allow com port controls to be changed by user once more
        ComPortSettingsControlsEnabled = true;

        StopLoggingButtonBackgroundColor = LightRed;
        StopLoggingButtonForegroundColor = TextDark;
        
        StartLoggingButtonBackgroundColor = DarkGreen;
        StartLoggingButtonForegroundColor = TextLight;
    }
    
    [RelayCommand]
    private void MainWindowOnClosing()
    {
        _logger.Information("Running {ThisName}, this is just an example message to show how to use MVVM behaviors from the XAML",
            System.Reflection.MethodBase.GetCurrentMethod()?.Name);
    }
    
    [RelayCommand]
    private void LoadDefaultPortSettings()
    {
        SelectedBaud = "115200";
        SelectedComPort = _serialPortHelpers.GetFirstSerialPortOnSystem();
        SelectedParity = "None";
        SelectedDataBits = "8";
        SelectedStopBits = "1";
    }
 
    private void LoadStoredSettingsIntoWindow()
    {
        // Load all options in settings .ini for each of the dropdown menus
        LoadCommaSeparatedOptions(BaudRates, _settingsApplicationLocal.SerialPossibleOptionsSettings.BaudRates);
        LoadCommaSeparatedOptions(ParityOptions, _settingsApplicationLocal.SerialPossibleOptionsSettings.ParityOptions);
        LoadCommaSeparatedOptions(DataBitOptions, _settingsApplicationLocal.SerialPossibleOptionsSettings.DataBits);
        LoadCommaSeparatedOptions(StopBitOptions, _settingsApplicationLocal.SerialPossibleOptionsSettings.StopBits);

        // Load last selected settings, if valid
        if (!string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastBaud))
            SelectedBaud = _settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastBaud;

        if (!string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastParity))
            SelectedParity = _settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastParity;

        if (!string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastDataBits))
            SelectedDataBits = _settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastDataBits;

        if (!string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastStopBits))
            SelectedStopBits = _settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastStopBits;

        if (!string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastComPort) &&
            _serialPortHelpers.SerialPortExists(_settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastComPort))
        {
            SelectedComPort = _settingsApplicationLocal.SerialPortSettingsSelectionsSelections.LastComPort;
        }
        
        // Log bytes as display selections
        IsCheckedLogAsAscii =
            _settingsApplicationLocal.SerialDataDisplayUserSelections.LastDisplayAsAsciiState;

        IsCheckedLogAsHex =
            _settingsApplicationLocal.SerialDataDisplayUserSelections.LastDisplayAsHexState;

        IsCheckedLogAsDecimal =
            _settingsApplicationLocal.SerialDataDisplayUserSelections.LastDisplayAsDecimalState;

        IsCheckedLogWithSpaces =
            _settingsApplicationLocal.SerialDataDisplayUserSelections.LastDisplayWithSpacesState;

        IsCheckedLogWithCommas =
            _settingsApplicationLocal.SerialDataDisplayUserSelections.LastDisplayWithCommasState;

        IsCheckedLogNewlineCharacters =
            _settingsApplicationLocal.SerialDataDisplayUserSelections.LastDisplayWithNewlineCharactersState;
        
        // Line ending detection settings
        IsCheckedLineEndingDetectionOnNewLine =
            _settingsApplicationLocal.LineEndingDetectionUserSelections.LastDetectNewlinesState;

        IsCheckedLineEndingDetectionOnHexValue =
            _settingsApplicationLocal.LineEndingDetectionUserSelections.LastDetectHexValueChecked;

        IsCheckedLineEndingDetectionOnDecimalValue =
            _settingsApplicationLocal.LineEndingDetectionUserSelections.LastDetectDecimalValueChecked;

        DecimalValueForLineEndingDetection =
            _settingsApplicationLocal.LineEndingDetectionUserSelections.LastDecimalCustomTextState;

        HexValueForLineEndingDetection =
            _settingsApplicationLocal.LineEndingDetectionUserSelections.LastHexCustomTextState;
        
        // Load directory to save logs to
        if (!string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialLogSettings.LastDirectory))
        {
            PathToSaveLogsIn = Path.GetFullPath(_settingsApplicationLocal.SerialLogSettings.LastDirectory);
        }
        else
        {
            BuiltLogFilename = "Please select logs directory first!";
            CurrentLogFilename = "Please select logs directory first!";
        }
    }
    
    private void InitializeNecessaryControls()
    {
        RescanSystemComPorts();
        
        SelectedComPort = _serialPortHelpers.GetFirstSerialPortOnSystem();

        StopLoggingButtonBackgroundColor = LightRed;
        StopLoggingButtonForegroundColor = TextDark; 
        
        StartLoggingButtonBackgroundColor = DarkGreen;
        StartLoggingButtonForegroundColor = TextLight; 
        
        // This looks fine for a default option, these will momentarily
        // be overwritten by the code that loads settings from configuration
        IsCheckedLogAsAscii = true;

        IsCheckedLineEndingDetectionOnNewLine = true;
    }
    
    private void LoadCommaSeparatedOptions(ICollection<string> listToLoad, string settingToSplit)
    {
        if (string.IsNullOrWhiteSpace(settingToSplit)) return;
        
        var splitSettings = settingToSplit.Split(",");
            
        foreach (var settingString in splitSettings)
        {
            listToLoad.Add(settingString.Trim());
        }
    }

    /// <summary>
    /// This is so we can just hide the window if we're running in Notification Tray Icon app mode
    /// </summary>
    /// <param name="sender">The main window</param>
    /// <param name="e">Cancel Event Args from the event</param>
    public void OnWindowClosing(object? sender, CancelEventArgs e) 
    {
        if (!ApplicationInformation.RunAsTrayIconApplication) return;
        
        e.Cancel = true;

        if (sender is null) return;
        
        ((Window)sender).Hide();
    }

    /// <summary>
    /// Called from the XAML when window is loaded
    /// </summary>
    public async Task OnWindowLoaded()
    {
        InitializeNecessaryControls();
        
        LoadStoredSettingsIntoWindow();

        while (true)
        {
            try
            {
                await UpdateLogTextboxWithFileContents();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            

            await Task.Delay(50);
        }
    }
}