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

                string code = await Task.Run(() =>
                {
                    try
                    {
                        return ClientManager.MatchClient.CreateLobby(hostPlayer, selectedPlayers, privacy);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
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

                    this.NavigationService.Navigate(new LobbyPage(code, Lang.GlobalTextPrivateMatch));
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(string.Format(Lang.ExceptionTextNoGameCreated, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
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
