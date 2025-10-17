using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ServiceModel;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrucoPrueba1.TrucoServer;

namespace TrucoPrueba1
{
    /// <summary>
    /// Lógica de interacción para TopTenPage.xaml
    /// </summary>
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
            InstanceContext context = new InstanceContext(new TrucoCallbackHandler());
            TrucoUserServiceClient client = new TrucoUserServiceClient(context, "NetTcpBinding_ITrucoUserService");
            try
            {
                var ranking = await client.GetGlobalRankingAsync();

                if (ranking != null && ranking.Any())
                {
                    dgRankings.ItemsSource = ranking.ToList();
                }
                else
                {
                    MessageBox.Show("No hay jugadores registrados aún.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (client.State != CommunicationState.Closed)
                {
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                if (client.State != CommunicationState.Closed && client.State != CommunicationState.Faulted)
                {
                    client.Abort();
                }

                string errorMessage = (ex is CommunicationException || ex is TimeoutException)
                    ? $"Error de conexión con el servidor: {ex.Message}"
                    : $"Error al cargar el ranking: {ex.Message}";

                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RankingsPage());
        }
    }
}
