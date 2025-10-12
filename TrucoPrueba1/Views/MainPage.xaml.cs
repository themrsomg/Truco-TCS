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
using System.Windows.Shapes;
using System.Xml.Linq;
using TrucoPrueba1.Properties.Langs;

namespace TrucoPrueba1
{
    /// <summary>
    /// Lógica de interacción para MainPage.xaml
    /// </summary>
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
            this.NavigationService.Navigate(new StartPage());

        }
        private void ClickProfile(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new UserProfilePage());
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
            this.NavigationService.Navigate(new FriendsPage());
        }
    }
}
