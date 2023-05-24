using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using SimpleSerialLoggerGui.Core;

namespace SimpleSerialLoggerGui.UI.WindowResources.MainWindow;

/// <summary>
/// The ViewModel for MainWindow
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _textRunningAsUsernameMessage = "";
    
    private readonly ILogger _logger;
    //private readonly ISettingsApplicationLocal _settingsApplicationLocal;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    // /// <param name="settingsApplicationLocal">ISettingsApplicationLocal proxy object from Config.net that was set up in DIContainerBuilder.cs</param>
    public MainWindowViewModel(ILogger logger) //, ISettingsApplicationLocal settingsApplicationLocal)
    {
        _logger = logger;
        // _settingsApplicationLocal = settingsApplicationLocal;

        //TextRunningAsUsernameMessage = $"Running As DomainUser: {Environment.UserDomainName} | User: {Environment.UserName}";
    }

    [RelayCommand]
    private void StartMainSetupProcess()
    {
        _logger.Information("Running {ThisName}", System.Reflection.MethodBase.GetCurrentMethod()?.Name);
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