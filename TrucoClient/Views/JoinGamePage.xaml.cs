using System;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Views;
using TrucoClient.Properties.Langs;

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
                    this.NavigationService.Navigate(new LobbyPage(code, Lang.GlobalTextPrivateMatch));
                }
                else
                {
                    MessageBox.Show(Lang.GameTextInvalidCode, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.GameTextErrorJoining, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }
    }
}
