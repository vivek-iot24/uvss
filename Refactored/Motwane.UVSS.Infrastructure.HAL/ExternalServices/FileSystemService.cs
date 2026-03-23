using System;
using System.IO;
using System.Runtime.InteropServices;
using Motwane.UVSS.Application.Interfaces.HAL;

namespace Motwane.UVSS.HAL.ExternalServices
{
    public class FileSystemService : IFileSystemService
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

        private const uint DELETE = 0x10000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint FILE_SHARE_DELETE = 0x00000004;
        private const uint OPEN_EXISTING = 3;

        public void DeleteAllFiles(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Directory does not exist: " + folderPath);
                return;
            }

            string[] files = Directory.GetFiles(folderPath);

            foreach (var file in files)
            {
                try
                {
                    IntPtr handle = CreateFile(
                        file,
                        DELETE,
                        FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
                        IntPtr.Zero,
                        OPEN_EXISTING,
                        0,
                        IntPtr.Zero);

                    if (handle != new IntPtr(-1))
                    {
                        File.Delete(file);
                        Console.WriteLine($"Deleted: {file}");
                    }
                    else
                    {
                        Console.WriteLine($"File in use or locked: {file}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete {file}: {ex.Message}");
                }
            }
        }
    }
}