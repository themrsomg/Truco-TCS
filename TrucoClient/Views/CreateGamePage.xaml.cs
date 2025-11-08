using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class CreateGamePage : Page
    {
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

                string code = await Task.Run(() =>
                    { 
                        return ClientManager.MatchClient.CreateLobby(hostPlayer, selectedPlayers, privacy);
                    }).ConfigureAwait(false);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (string.IsNullOrEmpty(code))
                    {
                        MessageBox.Show(Lang.WarningTextNoGameCreated, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (button != null)
                        {
                            button.IsEnabled = true;
                        }

                        return;
                    }

                    txtGeneratedCode.Text = code;
                    popupCode.IsOpen = true;

                    this.NavigationService.Navigate(new LobbyPage(code, matchName));
                });
            }
            catch (TimeoutException ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(Lang.ExceptionTextTimeoutCreatingMatch, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            catch (FormatException ex)
            {
                MessageBox.Show(Lang.ExceptionTextFormatErrorCreateMatch, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                MessageBox.Show(Lang.ExceptionTextConnectionError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextNoGameCreated, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (button != null)
                    {
                        button.IsEnabled = true;
                    }
                });
            }
        }

        private void ClickClosePopup(object sender, RoutedEventArgs e)
        {
            popupCode.IsOpen = false;
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }
    }
}
