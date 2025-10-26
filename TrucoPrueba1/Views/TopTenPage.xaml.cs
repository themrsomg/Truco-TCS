using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ServiceModel;
using TrucoPrueba1.TrucoServer;

namespace TrucoPrueba1
{
    public partial class TopTenPage : Page
    {
        public TopTenPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
            LoadTopTenPlayers();
        }
        private async void LoadTopTenPlayers()
        {
            try
            {
                var userClient = ClientManager.UserClient;
                var ranking = await userClient.GetGlobalRankingAsync();

                if (ranking != null && ranking.Any())
                {
                    dgRankings.ItemsSource = ranking.ToList();
                }
                else
                {
                    MessageBox.Show("No hay jugadores registrados aún.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.ServiceModel.EndpointNotFoundException ex)
            {
                MessageBox.Show($"No se pudo conectar al servidor: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RankingsPage());
        }
    }
}
