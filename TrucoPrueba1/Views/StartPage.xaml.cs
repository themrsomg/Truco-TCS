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
            string trackPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "Songs",
                "music_in_menus.mp3"
            );
            MusicManager.Play(trackPath);
            MusicManager.Volume = 0.3;
        }

        private void Language_Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (cbLanguages.SelectedIndex == 0)
            {
                Properties.Settings.Default.languageCode = "en-US";
            }
            else
            {
                Properties.Settings.Default.languageCode = "es-MX";
            }
            Properties.Settings.Default.Save();
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
