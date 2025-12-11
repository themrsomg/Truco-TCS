using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Services;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

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
            catch (FaultException<CustomFault> ex)
            {
                HandleFriendsFault(ex);
            }
            catch (TimeoutException ex)
            {
                ClientException.HandleError(ex, nameof(LoadTopTenPlayersAsync));
                CustomMessageBox.Show(Lang.ExceptionTextTimeout, MESSAGE_ERROR,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (EndpointNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(LoadTopTenPlayersAsync));
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError, MESSAGE_ERROR,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                ClientException.HandleError(ex, nameof(LoadTopTenPlayersAsync));
                CustomMessageBox.Show(Lang.ExceptionTextCommunication, MESSAGE_ERROR,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(LoadTopTenPlayersAsync));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void HandleFriendsFault(FaultException<CustomFault> ex)
        {
            switch (ex.Detail.ErrorCode)
            {
                case "ServerDBErrorRanking":
                    CustomMessageBox.Show(Lang.ExceptionTextDBErrorRanking, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case "ServerTimeout":
                    CustomMessageBox.Show(Lang.ExceptionTextTimeout, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RankingsPage());
        }
    }
}
