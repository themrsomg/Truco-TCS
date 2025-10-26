using System.Windows;
using System.Windows.Controls;
using TrucoPrueba1.Views;

namespace TrucoPrueba1
{
    public partial class PlayPage : Page
    {
        public PlayPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }

        private void ClickPlayFriends(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayWithFriendsPage());
        }

        private void ClickTrucoMatch(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SearchMatchPage());
        }
    }
}
