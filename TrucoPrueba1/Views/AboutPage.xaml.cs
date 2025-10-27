using System.Windows;
using System.Windows.Controls;

namespace TrucoClient
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
