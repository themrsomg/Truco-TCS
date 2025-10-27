using System.Windows;
using System.Windows.Controls;

namespace TrucoClient.Views
{
    public partial class GuestFriendsPage : Page
    {
        public GuestFriendsPage()
        {
            InitializeComponent();
        }
        private void ClickLogIn(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogInPage());
        }

        private void ClickSingUp(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new NewUserPage());
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }
    }
}
