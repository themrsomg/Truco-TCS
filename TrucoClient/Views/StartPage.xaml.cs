using System;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Session;


namespace TrucoClient.Views
{
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();
            SessionManager.Clear();

            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickLogIn(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogInPage());
        }

        private void ClickPlayAsGuest(object sender, RoutedEventArgs e)
        {
            string guestName = "Guest_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            SessionManager.CurrentUsername = guestName;
            this.NavigationService.Navigate(new MainPage());
        }

        private void ClickSingUp(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new NewUserPage());

        }

        private void ClickExit(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
    }
}
