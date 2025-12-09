using System;
using System.Threading.Tasks;
using System.Windows;
using System.ServiceModel;
using System.Windows.Controls;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.DTOs;

namespace TrucoClient.Views
{
    public partial class CreateGamePage : Page
    {
        private const string MESSAGE_ERROR = "Error";
        
        public CreateGamePage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void ClickCreateMatch(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            try
            {
                if (button != null)
                {
                    button.IsEnabled = false;
                }

                string hostPlayer = SessionManager.CurrentUsername;
                int selectedPlayers = int.Parse(((ComboBoxItem)cbPlayers.SelectedItem).Tag.ToString());
                string privacy = ((ComboBoxItem)cbPrivacy.SelectedItem).Tag.ToString();
                string matchName = string.Format(Lang.PreGameTextHostLobby, hostPlayer);

                string code = await ClientManager.MatchClient.CreateLobbyAsync(hostPlayer, selectedPlayers, privacy);

                if (string.IsNullOrEmpty(code))
                {
                    CustomMessageBox.Show(Lang.WarningTextNoGameCreated, MESSAGE_ERROR, 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                        
                    if (button != null)
                    {
                        button.IsEnabled = true;
                    }

                    return;
                }

                var arguments = new LobbyNavigationArguments
                {
                    MatchCode = code,
                    MatchName = matchName,
                    MaxPlayers = selectedPlayers,
                    IsPrivate = privacy == "private"
                };

                this.NavigationService.Navigate(new LobbyPage(arguments));

            }
            catch (TimeoutException ex)
            {
                ClientException.HandleError(ex, nameof(ClickCreateMatch));
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CustomMessageBox.Show(Lang.ExceptionTextTimeoutCreatingMatch, MESSAGE_ERROR, 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            catch (FormatException ex)
            {
                ClientException.HandleError(ex, nameof(ClickCreateMatch));
                CustomMessageBox.Show(Lang.ExceptionTextFormatErrorCreateMatch, MESSAGE_ERROR, 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (EndpointNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(ClickCreateMatch));
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError, MESSAGE_ERROR,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                ClientException.HandleError(ex, nameof(ClickCreateMatch));
                CustomMessageBox.Show(Lang.ExceptionTextCommunication, MESSAGE_ERROR, 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(ClickCreateMatch));
                CustomMessageBox.Show(Lang.ExceptionTextNoGameCreated, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (button != null)
                    {
                        button.IsEnabled = true;
                    }
                });
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }
    }
}
