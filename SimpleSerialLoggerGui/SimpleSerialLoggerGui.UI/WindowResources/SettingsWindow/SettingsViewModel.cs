using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SimpleSerialLoggerGui.UI.WindowResources.SettingsWindow;

/// <summary>
/// ViewModel for settings window
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    /// <summary>
    /// Bound for getting/showing the stored device instance path
    /// </summary>
    public string DeviceInstancePath { get; set; } = "";
    /// <summary>
    /// Bound for getting/showing the stored device's class GUID
    /// </summary>
    public string DeviceClassGuid { get; set; } = "";
    
    // Private
    //private readonly ISettingsApplicationLocal _settingsAppLocal;
    
    /// <summary>
    /// Constructor for dependency injection and checking that settings are initiliazed
    /// </summary>
    // /// <param name="settingsAppLocal">Injected application local settings to use</param>
    public SettingsViewModel() //ISettingsApplicationLocal settingsAppLocal)
    {
        //_settingsAppLocal = settingsAppLocal;
        
        InitializeFromConfig();
    }

    [RelayCommand]
    private void SaveSettings(object settingsWindowObject)
    {
        // Example
        //_settingsAppLocal.DeviceClassGuid = DeviceClassGuid;
        
        ((Window)settingsWindowObject).Hide();
    }

    [RelayCommand]
    private void Cancel(object settingsWindowObject)
    {
        var settingsWindow = (Window)settingsWindowObject;
        
        // Example reset setting to default
        // DeviceClassGuid = _settingsAppLocal.DeviceClassGuid;
        
        settingsWindow.Hide();
    }
    

    private void InitializeFromConfig()
    {
        // Example load setting on window load
        //DeviceClassGuid = _settingsAppLocal.DeviceClassGuid;
        
        // If there are required settings
        //if (!string.IsNullOrEmpty(DeviceClassGuid)) return;
        
        // Example warning to user if required settings are missing
        // Otherwise:
        // MessageBox.Show(
        //     "Required setting not initialized. " +
        //     "Please edit settings by right clicking on the tray icon and set these before using");
    }
}