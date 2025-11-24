using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;

namespace TrucoClient.Views
{
    public partial class CardsFourthPage : Page
    {
        public CardsFourthPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CardsThirdPage());
        }

        private void ClickExitToSettings(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }
    }
}
