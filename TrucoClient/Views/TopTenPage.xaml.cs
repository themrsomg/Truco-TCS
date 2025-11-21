using System;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.ServiceModel;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;

namespace TrucoClient.Views
{
    public partial class TopTenPage : Page
    {
        private const string MESSAGE_ERROR = "Error";   
        public TopTenPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
            _ = LoadTopTenPlayersAsync();
        }
        private async Task LoadTopTenPlayersAsync()
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
                    CustomMessageBox.Show(Lang.RankingsTextNoPlayersRegistered, Lang.GlobalTextInformation, 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (EndpointNotFoundException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError,
                    Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RankingsPage());
        }
    }
}
