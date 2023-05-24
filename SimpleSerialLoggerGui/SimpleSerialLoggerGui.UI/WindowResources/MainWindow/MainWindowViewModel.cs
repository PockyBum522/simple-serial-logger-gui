using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using SimpleSerialLoggerGui.Core.Interfaces;
using SimpleSerialLoggerGui.Core.Models;
using SimpleSerialLoggerGui.UI.WpfHelpers;

namespace SimpleSerialLoggerGui.UI.WindowResources.MainWindow;

/// <summary>
/// The ViewModel for MainWindow
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _textRunningAsUsernameMessage = "";
    
    private readonly ILogger _logger;
    private readonly ISettingsApplicationLocal _settingsApplicationLocal;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    /// <param name="settingsApplicationLocal">ISettingsApplicationLocal proxy object from Config.net that was set up in DIContainerBuilder.cs</param>
    public MainWindowViewModel(
        ILogger logger,
        ISettingsApplicationLocal settingsApplicationLocal)
    {
        _logger = logger;
        _settingsApplicationLocal = settingsApplicationLocal;

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
}