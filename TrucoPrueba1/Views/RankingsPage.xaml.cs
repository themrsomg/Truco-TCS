using System.Windows;
using System.Windows.Controls;

namespace TrucoPrueba1
{
    public partial class RankingsPage : Page
    {
        public RankingsPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickTopTen(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new TopTenPage());
        }
        private void ClickLastFiveGames(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LastMatchPage());
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }
    }
}
