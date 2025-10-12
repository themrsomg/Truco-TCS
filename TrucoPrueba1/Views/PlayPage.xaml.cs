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

namespace TrucoPrueba1
{
    /// <summary>
    /// Lógica de interacción para PlayPage.xaml
    /// </summary>
    public partial class PlayPage : Page
    {
        public PlayPage()
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

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }

        private void btnPartiV1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PreGamePage());
        }

        private void btnPartiV2(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PreGamePage());
        }

        private void btnPlayWFriends(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new JoinGamePage());
        }
    }
}
