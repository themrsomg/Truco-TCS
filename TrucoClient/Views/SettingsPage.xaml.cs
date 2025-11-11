using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TrucoClient.Properties;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Localization;

namespace TrucoClient.Views
{
    public partial class SettingsPage : Page
    {
        private const String VOLUME_ICON_MUTED_PATH = "/Resources/Logos/logo_muted.png";
        private const String VOLUME_ICON_VOLUME_PATH = "/Resources/Logos/logo_volume.png";

        public SettingsPage()
        {
            InitializeComponent();

            MusicInitializer.InitializeMenuMusic();
            UpdateVolumeIcon();

            SetInitialLanguage();
        }

        public SettingsPage(int selectedLanguageIndex)
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
            UpdateVolumeIcon();

            cbLanguages.SelectedIndex = selectedLanguageIndex;
        }

        private void UpdateVolumeIcon()
        {
            string iconPath = MusicManager.IsMuted
                ? VOLUME_ICON_MUTED_PATH
                : VOLUME_ICON_VOLUME_PATH;

            imgVolumeIcon.Source = new BitmapImage(new Uri(iconPath, UriKind.Relative));
        }

        private void ClickVolume(object sender, RoutedEventArgs e)
        {
            MusicManager.ToggleMute();
            UpdateVolumeIcon();
        }

        private void ClickAbout(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new AboutPage());
        }

        private void ClickCredits(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CreditsPage());
        }

        private void ClickSave(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsMusicMuted = MusicManager.IsMuted;

            Settings.Default.Save();

            this.NavigationService.Navigate(new MainPage());
        }

        private void SetInitialLanguage()
        {
            string currentLang = Settings.Default.languageCode;
            if (currentLang == "es-MX")
            {
                cbLanguages.SelectedIndex = 1;
            }
            else
            {
                cbLanguages.SelectedIndex = 0;
            }
        }

        private void LanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbLanguages.SelectedItem == null)
            {
                return;
            }

            int selectedIndex = cbLanguages.SelectedIndex;
            string newLang = (selectedIndex == 0) ? "en-US" : "es-MX";

            if (Settings.Default.languageCode == newLang)
            {
                return;
            }

            LanguageManager.ChangeLanguage(newLang);

            this.NavigationService.Navigate(new SettingsPage(selectedIndex));
        }
    }
}
