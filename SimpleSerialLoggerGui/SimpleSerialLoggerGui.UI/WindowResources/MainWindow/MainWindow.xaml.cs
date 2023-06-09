﻿using System.Windows;

namespace SimpleSerialLoggerGui.UI.WindowResources.MainWindow;

///<summary>
///Interaction logic for MainWindow.xaml
///</summary>
public partial class MainWindow
{
    /// <summary>
    /// Main window constructor
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        ((MainWindowViewModel)DataContext).OnWindowLoaded();
    }
}