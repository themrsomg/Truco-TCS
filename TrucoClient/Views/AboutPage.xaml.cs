using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;

namespace TrucoClient.Views
{
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }
    }
}
