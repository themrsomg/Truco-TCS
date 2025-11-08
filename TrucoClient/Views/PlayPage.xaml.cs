using System.Windows;
using System.Windows.Controls;
using TrucoClient.Views;

namespace TrucoClient
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

        private void ClickCustomMatch(object sender, RoutedEventArgs e)
        {
            if (SessionManager.CurrentUserData == null || SessionManager.CurrentUsername.StartsWith("Guest_"))
            {
                this.NavigationService.Navigate(new GuestCustomMatchPage());
            }
            else
            {
                this.NavigationService.Navigate(new PlayWithFriendsPage());
            }
        }

        private void ClickSearchMatch(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SearchMatchPage());
        }
    }
}
