using System;
using System.IO;

namespace SimpleSerialLoggerGui.Core
{
    /// <summary>
    /// Contains the few paths for this application that must be hardcoded
    /// </summary>
    public static class ApplicationPaths
    {
        /// <summary>
        /// Log folder path
        /// </summary>
        public static string LogAppBasePath =>
            Path.Combine(
                "C:",
                "Users",
                "Public",
                "Documents",
                ApplicationInformation.ApplicationName,
                "Logs",
                "Application");
        
        /// <summary>
        /// Settings folder path
        /// </summary>
        public static string PathSettingsApplicationLocalIniFile =>
            Path.Combine(
                "C:",
                "Users",
                "Public",
                "Documents",
                ApplicationInformation.ApplicationName,
                "Settings",
                "GeneralSettings.ini");

        /// <summary>
        /// Actual log file path passed to the ILogger configuration
        /// </summary>
        public static string LogPath =>
            Path.Combine(
                LogAppBasePath,
                "Application.log");
        
        /// <summary>
        /// The directory the assembly is running from
        /// </summary>
        public static string ThisApplicationRunFromDirectoryPath => 
            Path.GetDirectoryName(Environment.ProcessPath) ?? "";
        
        /// <summary>
        /// The full path to this application's running assembly
        /// </summary>
        public static string ThisApplicationProcessPath => 
            Environment.ProcessPath ?? "";

        /// <summary>
        /// The full path to the dark theme Styles.xaml which contains the rest of the style information
        /// </summary>
        public static string DarkThemePath =>
                Path.Join(
                    ThisApplicationRunFromDirectoryPath,
                    "Themes",
                    "SelenMetroDark",
                    "Styles.xaml");
    }
}
