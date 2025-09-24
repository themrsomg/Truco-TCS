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
        }
        private void ClickButtonRankings(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RankingsPage());

        }
        private void ClickButtonLogOut(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new StartPage());

        }
    }
}
