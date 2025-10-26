using System.Windows;
using System.Windows.Controls;

namespace TrucoPrueba1
{
    public partial class LastMatchPage : Page
    {
        public LastMatchPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RankingsPage());
        }
    }
}
