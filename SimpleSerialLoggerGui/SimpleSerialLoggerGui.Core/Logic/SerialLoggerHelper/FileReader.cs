using System;
using System.Text;

namespace SimpleSerialLoggerGui.Core.Logic.SerialLoggerHelper;

public class FileReader
{
    public string ReadFileWithoutLocking(string filePathToRead)
    {
        byte[]? oFileBytes;

        var stringBuilder = new StringBuilder(2048);

        using var fs = System.IO.File.Open(filePathToRead, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
        
        var numBytesToRead = Convert.ToInt32(fs.Length);
        
        oFileBytes = new byte[(numBytesToRead)];
        
        fs.Read(oFileBytes, 0, numBytesToRead);

        return Encoding.Default.GetString(oFileBytes);
    }
}