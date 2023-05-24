using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SimpleSerialLoggerGui.Core.Models;

/// <summary>
/// This holds our main window checkboxes state and available installers loaded from json
/// </summary>
public partial class SessionPersistentState : ObservableObject
{
    [ObservableProperty] private string _textHostname = "";
    
    // Power values from user
    [ObservableProperty] private string _textMonitorTimeoutOnAc = "";
    [ObservableProperty] private string _textMonitorTimeoutOnBattery = "";
    [ObservableProperty] private string _textStandbyTimeoutOnAc = "";
    [ObservableProperty] private string _textStandbyTimeoutOnBattery = "";
    [ObservableProperty] private string _textHibernateTimeoutOnAc = "";
    [ObservableProperty] private string _textHibernateTimeoutOnBattery = "";
    
    // Update windows
    [ObservableProperty] private bool _isCheckedUpdateWindows;
    
    /// <summary>
    /// Friendlier readout of helpful information for debugging
    /// </summary>
    /// <returns>Information about some of the properties and installers</returns>
    public override string ToString()
    {
        var returnString = $"IsCheckedUpdateWindows: {IsCheckedUpdateWindows}" + Environment.NewLine +

                           $"TextMonitorTimeoutOnAc: {TextMonitorTimeoutOnAc}" + Environment.NewLine;
        
        return returnString;
    }
}