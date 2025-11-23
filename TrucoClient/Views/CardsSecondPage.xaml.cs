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
using TrucoClient.Helpers.Audio;

namespace TrucoClient.Views
{
    /// <summary>
    /// Lógica de interacción para CardsSecondPage.xaml
    /// </summary>
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
