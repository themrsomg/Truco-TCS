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
using TrucoPrueba1.Properties.Langs;
using TrucoPrueba1.TrucoServer;

namespace TrucoPrueba1.Views
{
    /// <summary>
    /// Lógica de interacción para ForgotPasswordPage.xaml
    /// </summary>
    public partial class ForgotPasswordStepOnePage : Page
    {
        public ForgotPasswordStepOnePage()
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

        private void ClickButtonSendCode(object sender, RoutedEventArgs e)
        {

        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new StartPage());
        }
    }
}
