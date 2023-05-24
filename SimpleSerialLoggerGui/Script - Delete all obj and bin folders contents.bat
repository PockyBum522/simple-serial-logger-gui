REM -------========== SimpleSerialLoggerGui.Core ==========-------

set folder=".\SimpleSerialLoggerGui.Core\bin"

cd /d %folder%

for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q 2>NUL || del "%%i" /s/q >NUL )

set folder="..\obj"

cd /d %folder%

for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q 2>NUL || del "%%i" /s/q >NUL )

REM  -------========== SimpleSerialLoggerGui.Main ==========-------

set folder="..\..\SimpleSerialLoggerGui.Main\bin"

cd /d %folder%

for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q 2>NUL || del "%%i" /s/q >NUL )

set folder="..\obj"

cd /d %folder%

for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q 2>NUL || del "%%i" /s/q >NUL )

REM  -------========== SimpleSerialLoggerGui.UI ==========-------

set folder="..\..\SimpleSerialLoggerGui.UI\bin"

cd /d %folder%

for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q 2>NUL || del "%%i" /s/q >NUL )

set folder="..\obj"

cd /d %folder%

for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q 2>NUL || del "%%i" /s/q >NUL )

pause
