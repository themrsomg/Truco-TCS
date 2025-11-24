using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;

namespace TrucoClient.Views
{
    public partial class CardsSecondPage : Page
    {
        public CardsSecondPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CardsFirstPage());
        }

        private void ClickNextPage(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CardsThirdPage());
        }
    }
}
