using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;

namespace TrucoClient.Views
{
    public partial class CardsFirstPage : Page
    {
        public CardsFirstPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }

        private void ClickNextPage(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CardsSecondPage());
        }
    }
}
