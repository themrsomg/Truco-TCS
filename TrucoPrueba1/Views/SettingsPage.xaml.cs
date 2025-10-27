using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TrucoClient.Properties;

namespace TrucoClient
{
    public partial class SettingsPage : Page
    {
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
                ? "/Resources/logo_muted.png"
                : "/Resources/logo_volume.png";

            VolumeIcon.Source = new BitmapImage(new Uri(iconPath, UriKind.Relative));
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
