using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;

namespace TrucoClient.Views
{
    public partial class JoinGamePage : Page
    {
        private const string MESSAGE_ERROR = "Error";
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
                    CustomMessageBox.Show(Lang.GameTextInvalidCode, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.GameTextErrorJoining, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }

        private void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender == txtCode)
            {
                ClickJoin(btnJoin, null);
                e.Handled = true;
            }
        }
    }
}
