using System;
using System.Text;

namespace SimpleSerialLoggerGui.Core.Logic.SerialLoggerHelper;

/// <summary>
/// Methods for reading a file without locking it
/// </summary>
public class FileReader
{
    /// <summary>
    /// Reads a file without locking it
    /// </summary>
    /// <param name="filePathToRead">Full path to the file to read</param>
    /// <returns>File contents</returns>
    public string ReadFileWithoutLocking(string filePathToRead)
    {
        byte[]? oFileBytes;

        using var fs = System.IO.File.Open(filePathToRead, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
        
        var numBytesToRead = Convert.ToInt32(fs.Length);
        
        oFileBytes = new byte[(numBytesToRead)];
        
        fs.Read(oFileBytes, 0, numBytesToRead);

        return Encoding.Default.GetString(oFileBytes);
    }
}