using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Wallop
{
    public class UserSettings
    {
        public string PreferredResolution { get; set; } = "UHD";
        public List<string> DownloadHistory { get; set; } = new List<string>();
        public string Language { get; set; } = "en-US";
        public bool AutoDownload { get; set; } = false;
        public int MaxLoadedImages { get; set; } = 50;
        public string DownloadFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public bool AutoChangeEnabled { get; set; } = false;
        public string ChangeFrequency { get; set; } = "Günlük";
        public int ChangeIntervalSeconds { get; set; } = 86400;

        private static string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wallop", "settings.json");

        public void Save()
        {
            string directoryPath = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static UserSettings Load()
        {
            if (File.Exists(SettingsPath))
            {
                var settings = JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(SettingsPath));

                if (settings.MaxLoadedImages <= 0)
                {
                    settings.MaxLoadedImages = 50;
                }

                return settings;
            }

            return new UserSettings();
        }

        public void AddToDownloadHistory(string imageId)
        {
            if (!DownloadHistory.Contains(imageId))
            {
                DownloadHistory.Add(imageId);
                if (DownloadHistory.Count > 100)
                {
                    DownloadHistory.RemoveAt(0);
                }
                Save();
            }
        }

        public bool IsImageDownloaded(string imageId)
        {
            return DownloadHistory.Contains(imageId);
        }
    }
}