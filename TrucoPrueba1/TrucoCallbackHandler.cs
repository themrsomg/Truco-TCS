using System.Windows;
using TrucoPrueba1.Properties.Langs;

namespace TrucoPrueba1.TrucoServer
{
    public class TrucoCallbackHandler : ITrucoUserServiceCallback, ITrucoFriendServiceCallback, ITrucoMatchServiceCallback
    {
        public void OnPlayerJoined(string matchCode, string player)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is InitialWindows main &&
                    main.MainFrame.Content is GamePage gamePage)
                {
                    gamePage.ReceiveChatMessage(" ", string.Format(Lang.ChatTextPlayerJoined, player));
                }
            });
        }

        public void OnPlayerLeft(string matchCode, string player)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is InitialWindows main &&
                    main.MainFrame.Content is GamePage gamePage)
                {
                    gamePage.ReceiveChatMessage(" ", string.Format(Lang.ChatTextPlayerLeft, player));
                }
            });
        }
        public void OnCardPlayed(string matchCode, string player, string card) { }
        public void OnChatMessage(string matchCode, string player, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is InitialWindows main)
                {
                    if (main.MainFrame.Content is GamePage gamePage)
                    {
                        gamePage.ReceiveChatMessage(player, message);
                    }
                }
            });
        }

        public void OnMatchStarted(string matchCode) { }
        public void OnMatchEnded(string matchCode, string winner) { }

        public void OnFriendRequestReceived(string fromUser)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(string.Format(Lang.NotificactionDialogTextFriendRequest, fromUser), Lang.NotificactionDialogTextFriendRequestTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public void OnFriendRequestAccepted(string fromUser)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(string.Format(Lang.NotificactionDialogTextFriendAccepted, fromUser), Lang.NotificactionDialogTextFriendAcceptedTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
        public void MatchFound(string matchDetails)
        {
            // Lógica para cuando se encuentra una partida
        }

        public void PlayerJoined(string username)
        {
            // Lógica para cuando un jugador se une
        }
    }
}
