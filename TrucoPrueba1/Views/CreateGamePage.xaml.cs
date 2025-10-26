using System;
using System.Windows;
using System.Windows.Controls;

namespace TrucoPrueba1.Views
{
    /// <summary>
    /// Lógica de interacción para CreateGamePage.xaml
    /// </summary>
    public partial class CreateGamePage : Page
    {
        private static readonly Random random = new Random();
        public CreateGamePage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickInviteFriend(object sender, RoutedEventArgs e)
        {
            string codigo = GenerateCode(6);
            txtGeneratedCode.Text = codigo;
            popupCode.IsOpen = true;
        }
        private void ClickClosePopup(object sender, RoutedEventArgs e)
        {
            popupCode.IsOpen = false;
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }

        private string GenerateCode(int longitud)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] buffer = new char[longitud];

            for (int i = 0; i < longitud; i++)
            {
                buffer[i] = chars[random.Next(chars.Length)];
            }

            return new string(buffer);
        }
    }
}
