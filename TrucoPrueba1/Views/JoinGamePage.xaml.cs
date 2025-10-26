using System.Windows;
using System.Windows.Controls;

namespace TrucoPrueba1
{
    public partial class JoinGamePage : Page
    {
        public JoinGamePage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickUnirse(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new GamePage());
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }
    }
}
