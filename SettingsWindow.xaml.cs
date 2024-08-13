using System.Windows;
using Wpf.Ui.Controls;

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

            // MaxImagesNumberBox'un varsayılan değeri için kontrol
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

            if (MaxImagesNumberBox.Value.HasValue)
            {
                userSettings.MaxLoadedImages = (int)MaxImagesNumberBox.Value.Value;
            }
            else
            {
                userSettings.MaxLoadedImages = 50; // Varsayılan değer
            }

            userSettings.Save();
            DialogResult = true;
            Close();
        }
    }
}