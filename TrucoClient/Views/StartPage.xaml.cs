using System.Windows;
using System.Windows.Controls;

namespace TrucoClient
{
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();
            SessionManager.ClearSession();
        }

        private void LanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbLanguages.SelectedIndex == 0)
            {
                LanguageManager.ChangeLanguage("en-US");
            }
            else
            {
                LanguageManager.ChangeLanguage("es-MX");
            }
        }
        private void ClickLogIn(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogInPage());
        }
        private void ClickPlayAsGuest(object sender, RoutedEventArgs e)
        {
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
