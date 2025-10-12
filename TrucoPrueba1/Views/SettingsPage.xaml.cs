using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrucoPrueba1.Properties;

namespace TrucoPrueba1
{
    /// <summary>
    /// Lógica de interacción para SettingsPage.xaml
    /// </summary>
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
