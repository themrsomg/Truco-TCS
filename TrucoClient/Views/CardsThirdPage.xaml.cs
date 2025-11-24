using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;

namespace TrucoClient.Views
{
    public partial class CardsThirdPage : Page
    {
        public CardsThirdPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();

        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CardsSecondPage());
        }

        private void ClickNextPage(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CardsFourthPage());
        }
    }
}
