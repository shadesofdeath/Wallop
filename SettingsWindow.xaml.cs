using System;
using System.ServiceProcess;
using System.Windows;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace Wallop
{
    public partial class SettingsWindow : FluentWindow
    {
        private UserSettings userSettings;

        public SettingsWindow(UserSettings settings)
        {
            InitializeComponent();
            userSettings = settings;
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            ResolutionComboBox.SelectedIndex = GetResolutionIndex(userSettings.PreferredResolution);
            DownloadFolderTextBox.Text = userSettings.DownloadFolder;
            MaxImagesNumberBox.Value = userSettings.MaxLoadedImages > 0 ? userSettings.MaxLoadedImages : 50;
        }

        private int GetResolutionIndex(string resolution)
        {
            switch (resolution)
            {
                case "UHD": return 0;
                case "1920x1200": return 1;
                case "1920x1080": return 2;
                case "1366x768": return 3;
                case "1280x768": return 4;
                case "1024x768": return 5;
                default: return 0;
            }
        }

        private string GetResolutionValue(int index)
        {
            switch (index)
            {
                case 0: return "UHD";
                case 1: return "1920x1200";
                case 2: return "1920x1080";
                case 3: return "1366x768";
                case 4: return "1280x768";
                case 5: return "1024x768";
                default: return "UHD";
            }
        }

        private int GetFrequencyIndex(string frequency)
        {
            switch (frequency)
            {
                case "Günlük": return 0;
                case "Haftalık": return 1;
                case "Aylık": return 2;
                default: return 0;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                DownloadFolderTextBox.Text = dialog.SelectedPath;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            userSettings.PreferredResolution = GetResolutionValue(ResolutionComboBox.SelectedIndex);
            userSettings.DownloadFolder = DownloadFolderTextBox.Text;
            userSettings.MaxLoadedImages = (int)MaxImagesNumberBox.Value;
            userSettings.Save();

            ControlWallpaperChangerService();

            DialogResult = true;
            Close();
        }

        private void ControlWallpaperChangerService()
        {
            try
            {
                using (ServiceController service = new ServiceController("WallpaperChangerService"))
                {
                    if (userSettings.AutoChangeEnabled)
                    {
                        if (service.Status != ServiceControllerStatus.Running)
                        {
                            service.Start();
                            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                        }
                    }
                    else
                    {
                        if (service.Status == ServiceControllerStatus.Running)
                        {
                            service.Stop();
                            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hizmet kontrolü sırasında bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}