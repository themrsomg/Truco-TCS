using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrucoPrueba1.Properties.Langs;

namespace TrucoPrueba1
{
    /// <summary>
    /// Lógica de interacción para StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();
            SessionManager.ClearSession();
            MusicInitializer.InitializeMenuMusic();
        }

        private void Language_Selection_Changed(object sender, SelectionChangedEventArgs e)
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
    }
}
