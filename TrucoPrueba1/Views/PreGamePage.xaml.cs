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
    /// Lógica de interacción para PreGamePage.xaml
    /// </summary>
    public partial class PreGamePage : Page
    {
        private static readonly Random random = new Random();
        public PreGamePage()
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

        private void ClickBuscarPartida(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new GamePage());
        }

        private void ClickInvitarAmigo(object sender, RoutedEventArgs e)
        {
            string codigo = GenerarCodigo(6);
            txtCodigoGenerado.Text = codigo;
            popupCodigo.IsOpen = true;
        }
        private void ClickCerrarPopup(object sender, RoutedEventArgs e)
        {
            popupCodigo.IsOpen = false;
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }

        private string GenerarCodigo(int longitud)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] buffer = new char[longitud];

            for (int i = 0; i < longitud; i++)
                buffer[i] = chars[random.Next(chars.Length)];

            return new string(buffer);
        }
    }
}
