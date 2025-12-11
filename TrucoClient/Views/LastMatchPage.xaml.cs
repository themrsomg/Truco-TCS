using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class LastMatchPage : Page
    {
        private const string MESSAGE_ERROR = "Error";
        private const string DATE_FORMAT = "dd/MM/yyyy HH:mm";
        public LastMatchPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
            _ = LoadLastMatchesAsync();
        }

        private async Task LoadLastMatchesAsync()
        {
            string username = SessionManager.CurrentUsername;

            if (string.IsNullOrEmpty(username))
            {
                CustomMessageBox.Show(Lang.GlobalTextSessionError, MESSAGE_ERROR, 
                    MessageBoxButton.OK, MessageBoxImage.Error);

                this.NavigationService.Navigate(new LogInPage());

                return;
            }

            try
            {
                var userClient = ClientManager.UserClient;
                var matches = await userClient.GetLastMatchesAsync(username);

                if (matches != null && matches.Any())
                {
                    var displayData = matches.Select(m => new
                    {
                        MatchID = $"#{m.MatchID}",
                        Result = m.IsWin ? Lang.GlobalTextWinner : Lang.GlobalTextLoser,
                        Date = m.EndedAt.ToString(DATE_FORMAT),
                        FinalScore = m.FinalScore
                    }).ToList();
                    dgRankings.ItemsSource = displayData;
                }
                else
                {
                    CustomMessageBox.Show(Lang.DialogTextNoMatches, Lang.GlobalTextInformation, 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (FaultException<CustomFault> ex)
            {
                HandleFriendsFault(ex);
            }
            catch (TimeoutException ex)
            {
                ClientException.HandleError(ex, nameof(LoadLastMatchesAsync));
                CustomMessageBox.Show(Lang.ExceptionTextTimeout, MESSAGE_ERROR,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (EndpointNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(LoadLastMatchesAsync));
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError, MESSAGE_ERROR,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                ClientException.HandleError(ex, nameof(LoadLastMatchesAsync));
                CustomMessageBox.Show(Lang.ExceptionTextCommunication, MESSAGE_ERROR,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(LoadLastMatchesAsync));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void HandleFriendsFault(FaultException<CustomFault> ex)
        {
            switch (ex.Detail.ErrorCode)
            {
                case "ServerDBErrorHistory":
                    CustomMessageBox.Show(Lang.ExceptionTextDBErrorHistory, MESSAGE_ERROR,
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
