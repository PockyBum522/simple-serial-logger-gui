using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using Serilog.Core;
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
    [ObservableProperty] private string _hexValueForLineEndingDetection;
    [ObservableProperty] private string _decimalValueForLineEndingDetection;

    [ObservableProperty] private bool _comPortSettingsControlsEnabled = true;
    
    [ObservableProperty] private ObservableCollection<string> _comPorts = new();
    [ObservableProperty] private ObservableCollection<string> _baudRates = new();
    [ObservableProperty] private ObservableCollection<string> _parityOptions = new();
    [ObservableProperty] private ObservableCollection<string> _dataBitOptions = new();
    [ObservableProperty] private ObservableCollection<string> _stopBitOptions = new();
    
    private readonly ILogger _logger;
    private readonly Dispatcher _uiThreadDispatcher;
    private readonly SerialLogger _serialLogger;
    private readonly ISettingsApplicationLocal _settingsApplicationLocal;
    private readonly SerialPortHelpers _serialPortHelpers;

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

        LoadStoredSettingsIntoWindow();

        InitializeNecessaryControls();

        Task.Run(UpdateLogTextboxWithFileContents);
    }

    private async Task UpdateLogTextboxWithFileContents()
    {
        var lastFileLinesCount = 0;
        var lastFileData = "";

        while (true)
        {
            var fileReader = new FileReader();
            
            var newestLogFilePath = FolderHelpers.GetNewestFileIn(PathToSaveLogsIn, "*.log");

            await Task.Delay(10);
            
            var fileData = fileReader.ReadFileWithoutLocking(newestLogFilePath);

            if (lastFileData == fileData) continue;

            lastFileData = fileData;

            _uiThreadDispatcher.Invoke(() =>
            {
                CurrentSerialLog = fileData;
            });
        }
    }

    [RelayCommand]
    private void StartNewLogFile()
    {
        if (string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialLogSettings.LastDirectory))
        {
            MessageBox.Show("Log to directory is empty or invalid. Please set a directory to log serial data to and try again.");
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedComPort))
        {
            MessageBox.Show("Please pick a serial port");
            return;
        }
        
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
        _serialLogger.OpenComPort(serialSettings, logFormatSettings);
        
        // Disable com port controls from changes allowed
        ComPortSettingsControlsEnabled = false;

        // If successful, start logging with BuiltLogFilename
        _serialLogger.StartLogging(
            Path.Join(PathToSaveLogsIn,
                      "serial_data_.log"));

        // Update CurrentLogFilename

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
        _serialLogger.CloseComPort();
        
        // Allow com port controls to be changed by user once more
        ComPortSettingsControlsEnabled = true;
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
        // Get all port names on system
        foreach (var portNames in _serialPortHelpers.GetAllSerialPortsOnSystem())
        {
            ComPorts.Add(portNames.Trim());
        }
        
        // Load all options in settings .ini for each of the dropdown menus
        LoadCommaSeparatedOptions(BaudRates, _settingsApplicationLocal.SerialPossibleOptionsSettings.BaudRates);
        LoadCommaSeparatedOptions(ParityOptions, _settingsApplicationLocal.SerialPossibleOptionsSettings.ParityOptions);
        LoadCommaSeparatedOptions(DataBitOptions, _settingsApplicationLocal.SerialPossibleOptionsSettings.DataBits);
        LoadCommaSeparatedOptions(StopBitOptions, _settingsApplicationLocal.SerialPossibleOptionsSettings.StopBits);

        // Load last selected settings, if valid
        if (!string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialSelectionSettings.LastBaud))
            SelectedBaud = _settingsApplicationLocal.SerialSelectionSettings.LastBaud;

        if (!string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialSelectionSettings.LastParity))
            SelectedParity = _settingsApplicationLocal.SerialSelectionSettings.LastParity;

        if (!string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialSelectionSettings.LastDataBits))
            SelectedDataBits = _settingsApplicationLocal.SerialSelectionSettings.LastDataBits;

        if (!string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialSelectionSettings.LastStopBits))
            SelectedStopBits = _settingsApplicationLocal.SerialSelectionSettings.LastStopBits;

        if (!string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialSelectionSettings.LastComPort) &&
            _serialPortHelpers.SerialPortExists(SelectedComPort = _settingsApplicationLocal.SerialSelectionSettings.LastComPort))
        {
            SelectedComPort = _settingsApplicationLocal.SerialSelectionSettings.LastComPort;

            return;
        }

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

        // Otherwise:
        SelectedComPort = "";
    }
    
    private void InitializeNecessaryControls()
    {
        SelectedComPort = _serialPortHelpers.GetFirstSerialPortOnSystem();

        // This looks fine for a default option
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
}