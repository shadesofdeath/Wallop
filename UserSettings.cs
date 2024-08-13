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

                // MaxLoadedImages için geçerlilik kontrolü
                if (settings.MaxLoadedImages <= 0)
                {
                    settings.MaxLoadedImages = 50; // Geçersiz değer varsa varsayılana çevir
                }

                return settings;
            }

            // JSON dosyası yoksa veya geçersizse, varsayılan ayarlarla yeni bir nesne döndür
            return new UserSettings();
        }


        public void AddToDownloadHistory(string imageId)
        {
            if (!DownloadHistory.Contains(imageId))
            {
                DownloadHistory.Add(imageId);
                if (DownloadHistory.Count > 100) // Maksimum 100 öğe sakla
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