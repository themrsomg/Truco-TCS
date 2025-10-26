using System.Windows;
using System.Windows.Controls;

namespace TrucoPrueba1.Views
{
    public partial class PlayWithFriendsPage : Page
    {
        public PlayWithFriendsPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickJoinMatch(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new JoinGamePage());
        }

        private void ClickCreateMatch(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CreateGamePage());
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }
    }
}
