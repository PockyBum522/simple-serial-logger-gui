namespace SimpleSerialLoggerGui.Core;

/// <summary>
/// Contains the few paths for this application that must be hardcoded
/// </summary>
public static class ApplicationInformation
{
    /// <summary>
    /// Friendly name for the application. Can only be set once. That should happen in App.xaml.cs
    /// </summary>
    public static string ApplicationFriendlyName
    {
        get => _applicationFriendlyName;
        set
        {
            if (_hasApplicationNameBeenSet) return;

            _applicationFriendlyName = value;

            _hasApplicationNameBeenSet = true;
        } 
    }
    
    /// <summary>
    /// Friendly name for the application. Can only be set once. That should happen in App.xaml.cs
    /// </summary>
    public static bool RunAsTrayIconApplication
    {
        get => _runAsTrayIconApplication;
        set
        {
            if (_hasRunAsTrayIconApplicationBeenSet) return;

            _runAsTrayIconApplication = value;
            
            _hasRunAsTrayIconApplicationBeenSet = true;
        } 
    }

    /// <summary>
    /// Name for the application but without spaces, derived from ApplicationFriendlyName
    /// </summary>
    public static string ApplicationNameNoSpaces => ApplicationFriendlyName.Replace(" ", "");

    private static string _applicationFriendlyName = "SimpleSerialLoggerGui";
    private static bool _hasApplicationNameBeenSet;

    private static bool _runAsTrayIconApplication = true;
    private static bool _hasRunAsTrayIconApplicationBeenSet;
}