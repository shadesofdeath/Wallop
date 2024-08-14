using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace Wallop
{
    public partial class MainWindow : FluentWindow
    {
        private ObservableCollection<Wallpaper> _wallpapers = new ObservableCollection<Wallpaper>();
        private HttpClient client = new HttpClient();
        private int currentIndex = 0;
        private bool isLoading = false;
        private UserSettings userSettings;

        public ObservableCollection<Wallpaper> Wallpapers
        {
            get
            {
                return _wallpapers;
            }
            set
            {
                _wallpapers = value;
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitializeCategoryComboBox();
            LoadUserSettings();
            _ = SearchWallpapers();
        }
        private void LoadUserSettings()
        {
            userSettings = UserSettings.Load();
            if (userSettings == null)
            {
                userSettings = new UserSettings();
                userSettings.Save();
            }
        }

        private void InitializeCategoryComboBox()
        {
            CategoryComboBox.Items.Add("Günlük");
            CategoryComboBox.Items.Add("Rastgele");
            CategoryComboBox.SelectedIndex = 0;
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            currentIndex = 0;
            Wallpapers.Clear();
            NoResultsTextBlock.Visibility = Visibility.Collapsed;
            await SearchWallpapers();
        }

        private async Task SearchWallpapers()
        {
            if (isLoading || Wallpapers.Count >= userSettings.MaxLoadedImages) return;
            isLoading = true;

            try
            {
                while (Wallpapers.Count < userSettings.MaxLoadedImages)
                {
                    string category = CategoryComboBox.SelectedIndex == 0 ? currentIndex.ToString() : "random";
                    string url = $"https://bingw.jasonzeng.dev?resolution={userSettings.PreferredResolution}&index={category}&format=json";

                    var response = await client.GetStringAsync(url);
                    var jsonResponse = JObject.Parse(response);

                    string thumbnailUrl = jsonResponse["url"].ToString().Replace(userSettings.PreferredResolution, "320x240");

                    Wallpapers.Add(new Wallpaper
                    {
                        Id = jsonResponse["startdate"].ToString(),
                        ThumbnailUrl = thumbnailUrl,
                        FullImageUrl = jsonResponse["url"].ToString(),
                        Resolution = userSettings.PreferredResolution,
                        FileSize = 0,
                        Category = jsonResponse["copyright"].ToString()
                    });

                    currentIndex++;

                    // Arayüzü güncellemek için Dispatcher kullanıyoruz
                    await Dispatcher.InvokeAsync(() =>
                    {
                        NoResultsTextBlock.Visibility = Wallpapers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Image image && image.DataContext is Wallpaper wallpaper)
            {
                var previewWindow = new PreviewWindow(wallpaper, userSettings);
                previewWindow.ShowDialog();
            }
        }

        private async void WallpaperScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                currentIndex++;
                await SearchWallpapers();
            }
        }

        private void OpenSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(userSettings);
            if (settingsWindow.ShowDialog() == true)
            {
                LoadUserSettings();
                // Ayarlar değiştiğinde resimleri yeniden yükle
                Wallpapers.Clear();
                currentIndex = 0;
                _ = SearchWallpapers();
            }
        }

        private void InfoWindow_Click(object sender, RoutedEventArgs e)
        {
            var ınfoWindow = new InfoWindow();
            ınfoWindow.ShowDialog();
        }
    }

    public class Wallpaper
    {
        public string Id
        {
            get; set;
        }
        public string ThumbnailUrl
        {
            get; set;
        }
        public string FullImageUrl
        {
            get; set;
        }
        public string Resolution
        {
            get; set;
        }
        public long FileSize
        {
            get; set;
        }
        public string Category
        {
            get; set;
        }
    }
}