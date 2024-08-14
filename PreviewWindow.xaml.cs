using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace Wallop
{
    public partial class PreviewWindow : Wpf.Ui.Controls.FluentWindow
    {
        private Wallpaper currentWallpaper;
        private UserSettings userSettings;

        public PreviewWindow(Wallpaper wallpaper, UserSettings settings)
        {
            InitializeComponent();
            currentWallpaper = wallpaper;
            userSettings = settings;
            LoadImage();
        }

        private async void LoadImage()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // En yüksek çözünürlüklü URL'yi kullan
                    string highResUrl = GetHighestResolutionUrl(currentWallpaper.FullImageUrl);

                    var imageBytes = await client.GetByteArrayAsync(highResUrl);
                    var image = new BitmapImage();
                    using (var mem = new MemoryStream(imageBytes))
                    {
                        mem.Position = 0;
                        image.BeginInit();
                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = null;
                        image.StreamSource = mem;
                        image.EndInit();
                    }
                    image.Freeze();
                    PreviewImage.Source = image;

                    // Belleği temizle
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Resim yüklenirken hata oluştu: {ex.Message}");
            }
        }
        private string GetHighestResolutionUrl(string url)
        {
            // Mevcut çözünürlükleri tanımla (en yüksekten en düşüğe)
            string[] resolutions = { "UHD", "1920x1200", "1920x1080", "1366x768", "1280x768", "1024x768" };

            foreach (var resolution in resolutions)
            {
                if (url.Contains(resolution))
                {
                    // En yüksek çözünürlüğü (UHD) kullan
                    return url.Replace(resolution, "UHD");
                }
            }

            // Eğer tanımlı çözünürlüklerden biri bulunamazsa, orijinal URL'yi döndür
            return url;
        }

        private async void SetWallpaper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string preferredResolutionUrl = GetPreferredResolutionUrl(currentWallpaper.FullImageUrl);
                    var imageBytes = await client.GetByteArrayAsync(preferredResolutionUrl);
                    string tempPath = Path.Combine(Path.GetTempPath(), $"wallpaper_{currentWallpaper.Id}.jpg");
                    File.WriteAllBytes(tempPath, imageBytes);
                    SetWallpaper(tempPath);
                    ShowInfoBar("Duvar kağıdı başarıyla ayarlandı.");
                }
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Duvar kağıdı ayarlanırken hata oluştu: {ex.Message}");
            }
        }
        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string preferredResolutionUrl = GetPreferredResolutionUrl(currentWallpaper.FullImageUrl);
                    var imageBytes = await client.GetByteArrayAsync(preferredResolutionUrl);
                    string downloadPath = Path.Combine(userSettings.DownloadFolder, $"wallpaper_{currentWallpaper.Id}.jpg");
                    File.WriteAllBytes(downloadPath, imageBytes);
                    ShowInfoBar($"Duvar kağıdı başarıyla indirildi: {downloadPath}");
                }
            }
            catch (Exception ex)
            {
                ShowInfoBar($"İndirme sırasında hata oluştu: {ex.Message}");
            }
        }
        private string GetPreferredResolutionUrl(string url)
        {
            // Mevcut çözünürlükleri tanımla
            string[] resolutions = { "UHD", "1920x1200", "1920x1080", "1366x768", "1280x768", "1024x768" };

            foreach (var resolution in resolutions)
            {
                if (url.Contains(resolution))
                {
                    // Kullanıcının tercih ettiği çözünürlüğü kullan
                    return url.Replace(resolution, userSettings.PreferredResolution);
                }
            }

            // Eğer tanımlı çözünürlüklerden biri bulunamazsa, orijinal URL'yi döndür
            return url;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private void SetWallpaper(string path)
        {
            const int SPI_SETDESKWALLPAPER = 20;
            const int SPIF_UPDATEINIFILE = 0x01;
            const int SPIF_SENDWININICHANGE = 0x02;
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        private void ShowInfoBar(string message)
        {
            InfoBar.Message = message;
            InfoBar.IsOpen = true;
        }
    }
}