using System.Windows;
using System.Windows.Controls;
using TrucoClient.Views;

namespace TrucoClient
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }
        private void ClickMatch(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }
        private void ClickRankings(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RankingsPage());
        }
        private void ClickLogOut(object sender, RoutedEventArgs e)
        {
            SessionManager.Clear();
            this.NavigationService.Navigate(new StartPage());

        }
        private void ClickProfile(object sender, RoutedEventArgs e)
        {
            if (SessionManager.CurrentUserData == null || SessionManager.CurrentUsername == "UsuarioActual")
            {
                this.NavigationService.Navigate(new GuestProfilePage());
            }
            else
            {
                this.NavigationService.Navigate(new UserProfilePage());
            }
        }
        private void ClickExit(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
        private void ClickSettings(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }
        private void ClickFriends(object sender, RoutedEventArgs e)
        {
            if (SessionManager.CurrentUserData == null || SessionManager.CurrentUsername == "UsuarioActual")
            {
                this.NavigationService.Navigate(new GuestFriendsPage());
            }
            else
            {
                this.NavigationService.Navigate(new FriendsPage());
            }
        }
    }
}
