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
    /// Lógica de interacción para SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void ClickVolume(object sender, RoutedEventArgs e)
        {
            ///TO DO: Implementación de acción de Mute y Unmute
        }

        private void ClickAbout(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new AboutPage());
        }

        private void ClickCredits(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CreditsPage());
        }

        private void ClickSave(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
            ///TO DO: Implementación de guardar cambios
        }
    }
}
