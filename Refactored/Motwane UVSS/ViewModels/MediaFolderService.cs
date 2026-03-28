using System;
using System.IO;

namespace Motwane.UVSS.ViewModels
{
    public class MediaFolderService
    {
        private readonly string basePath = @"D:\UVSS_MEDIA";

        public void EnsureStructure()
        {
            try
            {
                // Ensure root exists
                Directory.CreateDirectory(basePath);

                string[] mainFolders = { "Entry Media", "Exit Media" };

                string[] subFolders =
                {
                    "ANPR Images",
                    "Driver Images",
                    "Underside Images",
                    "Cropped Logo Images",
                    "Cropped Number Plate Images",
                    "Video Camera1",
                    "Video Camera2",
                    "Video Camera3"
                };

                DateTime now = DateTime.Now;

                string year = now.Year.ToString();
                string month = now.Month.ToString("D2");
                string day = now.Day.ToString("D2");

                foreach (var main in mainFolders)
                {
                    foreach (var sub in subFolders)
                    {
                        string fullPath = Path.Combine(
                            basePath,
                            main,
                            sub,
                            year,
                            month,
                            day
                        );

                        Directory.CreateDirectory(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log instead of crashing app
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
    }
}