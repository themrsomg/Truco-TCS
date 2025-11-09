using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TrucoClient.Properties;
using TrucoClient.Helpers.Audio;

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
            if (Settings.Default.IsMusicMuted)
            {
                MusicManager.ToggleMute();
            }
            UpdateVolumeIcon();
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
    }
}
