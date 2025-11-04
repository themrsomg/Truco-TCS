using System;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Views;

namespace TrucoClient
{
    public partial class JoinGamePage : Page
    {
        public JoinGamePage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickJoin(object sender, RoutedEventArgs e)
        {
            string code = txtCode.Text.Trim();
            string player = SessionManager.CurrentUsername;

            try
            {
                bool joined = ClientManager.MatchClient.JoinMatch(code, player);
                if (joined)
                {
                    this.NavigationService.Navigate(new LobbyPage(code, "Partida Privada"));
                }
                else
                {
                    MessageBox.Show("Código inválido o partida no disponible.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al unirse: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }
    }
}
