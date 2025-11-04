using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TrucoClient.Views
{
    public partial class CreateGamePage : Page
    {
        public CreateGamePage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void ClickInviteFriend(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            try
            {
                if (btn != null) btn.IsEnabled = false;

                string hostPlayer = SessionManager.CurrentUsername;

                string code = await Task.Run(() =>
                {
                    try
                    {
                        return ClientManager.MatchClient.CreateMatch(hostPlayer);
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
                        MessageBox.Show("No se pudo crear la partida.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (btn != null) btn.IsEnabled = true;
                        {
                            return;
                        }
                    }

                    txtGeneratedCode.Text = code;
                    popupCode.IsOpen = true;

                    this.NavigationService.Navigate(new LobbyPage(code, "Partida Privada"));
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error al crear la partida: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (btn != null) btn.IsEnabled = true;
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
