# simple-serial-logger-gui
WPF and .NET 7+ Simple serial port log to file

![image](https://github.com/PockyBum522/simple-serial-logger-gui/assets/1970959/c8a039cb-f8a9-4a6c-8beb-8585c6c773b0)

# What works

* Opening serial port with dropdown options
* Dropdown options populated from settings (See example configuration info below)
* Logging to a file (Read in realtime with VSCode)
* Display incoming serial data in "Serial log" textbox

* Logging as: 
  * ASCII
  * Hex
  * Decimal
  
* Logging with: Spaces as separator, commas as separator, both, none
* Logging newline characters or not

* Newline detection:
  * On '\n'
  * On custom decimal value

# What doesn't work

* Everything else

# Example Configuration File

(Place at C:\Users\Public\Documents\SimpleSerialLoggerGui\Settings\GeneralSettings.ini)

```
[SerialSelectionSettings]
LastComPort=COM1
LastBaud=115200
LastParity=None
LastDataBits=8
LastStopBits=1

[SerialLogSettings]
LastDirectory=D:\Dropbox\Documents\Desktop\Serial Logs

[SerialPossibleOptionsSettings]
StopBits=0, 1, 1.5, 2
BaudRates=4800, 9600, 19200, 115200
DataBits=6, 7, 8, 9
ParityOptions=None, Even, Odd, Mark, Space

[ApplicationSettings]
ShowMainWindowOnStartup=true

```
