using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using Serilog.Core;
using SimpleSerialLoggerGui.Core;
using SimpleSerialLoggerGui.Core.Interfaces;
using SimpleSerialLoggerGui.Core.Logic.SerialLoggerHelper;
using SimpleSerialLoggerGui.Core.Models;

namespace SimpleSerialLoggerGui.UI.WindowResources.MainWindow;

/// <summary>
/// The ViewModel for MainWindow
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _currentSerialLog = "";

    [ObservableProperty] private string _currentLogFilename = "";
    [ObservableProperty] private string _builtLogFilename = "";
    
    [ObservableProperty] private string _logFilenameOptionalAppend = "";
    
    [ObservableProperty] private string _selectedComPort = "";
    [ObservableProperty] private string _selectedBaud = "";
    [ObservableProperty] private string _selectedParity = "";
    [ObservableProperty] private string _selectedDataBits = "";
    [ObservableProperty] private string _selectedStopBit = "";

    [ObservableProperty] private bool _comPortSettingsControlsEnabled = true;
    
    [ObservableProperty] private List<string> _comPorts = new();
    [ObservableProperty] private List<string> _baudRates = new();
    [ObservableProperty] private List<string> _parityOptions = new();
    [ObservableProperty] private List<string> _dataBitOptions = new();
    [ObservableProperty] private List<string> _stopBitOptions = new();
    
    private readonly ILogger _logger;
    private readonly SerialLogger _serialLogger;
    private readonly ISettingsApplicationLocal _settingsApplicationLocal;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    /// <param name="serialLogger">Injected SerialLogger</param>
    /// <param name="settingsApplicationLocal">ISettingsApplicationLocal proxy object from Config.net that was set up in DIContainerBuilder.cs</param>
    public MainWindowViewModel(ILogger logger, SerialLogger serialLogger, ISettingsApplicationLocal settingsApplicationLocal)
    {
        _logger = logger;
        _serialLogger = serialLogger;
        _settingsApplicationLocal = settingsApplicationLocal;
    }

    [RelayCommand]
    private void StartNewLogFile()
    {
        if (string.IsNullOrWhiteSpace(_settingsApplicationLocal.SerialLogSettings.LastDirectory))
        {
            MessageBox.Show("Log to directory is empty or invalid. Please set a directory to log serial data to and try again.");
            
            return;
        }

        var serialSettings = new SerialPortSettings()
        {
            BaudRate = int.Parse(SelectedBaud),
            ComPortName = SelectedComPort,
            DataBits = int.Parse(SelectedDataBits),
            StopBits = SelectedStopBit,
            ParityOption = SelectedParity
        };
        
        // Open com port with user selected settings
        _serialLogger.OpenComPort(serialSettings);
        
        // Disable com port controls from changes allowed
        ComPortSettingsControlsEnabled = false;

        // If successful, start logging with BuiltLogFilename
        

        // Update CurrentLogFilename


    }
    
    [RelayCommand]
    private void StopLogging()
    {
        // Close com port with user selected settings
        _serialLogger.CloseComPort();
        
        // Allow com port controls to be changed by user once more
        
        // If successful, start logging with BuiltLogFilename

    }
    
    [RelayCommand]
    private void MainWindowOnClosing()
    {
        _logger.Information("Running {ThisName}, this is just an example message to show how to use MVVM behaviors from the XAML",
            System.Reflection.MethodBase.GetCurrentMethod()?.Name);
    }
    
    /// <summary>
    /// This is so we can just hide the window if we're running in Notification Tray Icon app mode
    /// </summary>
    /// <param name="sender">The main window</param>
    /// <param name="e">Cancel Event Args from the event</param>
    public void OnWindowClosing(object? sender, CancelEventArgs e) 
    {
        _logger.Information("Running {ThisName}, this is just an example message to show how to use MVVM behaviors from the XAML",
            System.Reflection.MethodBase.GetCurrentMethod()?.Name);

        if (!ApplicationInformation.RunAsTrayIconApplication) return;
        
        e.Cancel = true;

        if (sender is null) return;
        
        ((Window)sender).Hide();
    }
}