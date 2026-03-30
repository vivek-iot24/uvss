using System;
using System.IO;

namespace Motwane.UVSS.ViewModels
{
    public class MediaFolderService
    {
        private readonly string basePath = @"C:\Document\UVSS MEDIA";
        public void EnsureStructure()
        {
            try
            {
                Directory.CreateDirectory(basePath);
                string[] mainFolders = { "Entry Media","Exit Media"};
                string[] imageFolders =
                {   "ANPR Images",
                    "Driver Images",
                    "Underside Images",
                    "Cropped Logo Images",
                    "Cropped Number Plate Images"
                };
               string[] videoFolders =
                {
                    "Camera1",
                    "Camera2",
                    "Camera3"
                };
                DateTime now = DateTime.Now;
                string year = now.Year.ToString();
                string month = now.Month.ToString("D2");
                string day = now.Day.ToString("D2");
                 foreach (var main in mainFolders)
                {
                    foreach (var folder in imageFolders)
                    {
                        string fullPath = Path.Combine(basePath, main, folder, year, month, day);
                        Directory.CreateDirectory(fullPath);
                    }
                    foreach (var camera in videoFolders)
                    {
                        string videoPath = Path.Combine(basePath, main, "Video", camera, year, month, day);
                        Directory.CreateDirectory(videoPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Folder creation error: " + ex.Message);
            }
        }
        public string GetTodayPath(string mainFolder, string subFolder)
        {  
            DateTime now = DateTime.Now;
            string path = Path.Combine(
                basePath,
                mainFolder,
                subFolder,
                now.Year.ToString(),
                now.Month.ToString("D2"),
                now.Day.ToString("D2")
            );
            Directory.CreateDirectory(path);
            return path;
        }

        public string GetTodayVideoPath(string mainFolder, string cameraName)
        {
            DateTime now = DateTime.Now;
            string path = Path.Combine(
                basePath,
                mainFolder,
                "Video",
                cameraName,
                now.Year.ToString(),
                now.Month.ToString("D2"),
                now.Day.ToString("D2")
            );
            Directory.CreateDirectory(path);
            return path;
        }
    }
}