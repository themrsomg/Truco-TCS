using System.Windows;
using TrucoClient.Properties.Langs;
using TrucoClient.Views;

namespace TrucoClient.TrucoServer
{
    public class TrucoCallbackHandler : ITrucoUserServiceCallback, ITrucoFriendServiceCallback, ITrucoMatchServiceCallback
    {
        public void OnPlayerJoined(string matchCode, string player)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (App.Current.MainWindow is InitialWindows main &&
                    main.MainFrame.Content is LobbyPage lobby)
                {
                    lobby.AddChatMessage(string.Empty, string.Format(Lang.CallbacksTextPlayerJoinedLobby, player));
                    lobby.ReloadPlayersDeferred();
                }
            });
        }

        public void OnPlayerLeft(string matchCode, string player)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is InitialWindows main &&
                    main.MainFrame.Content is LobbyPage lobbyPage)
                {
                    lobbyPage.AddChatMessage(string.Empty, string.Format(Lang.CallbacksTextPlayerLeftLobby, player));
                    lobbyPage.ReloadPlayersDeferred();
                }
            });
        }

        public void OnMatchStarted(string matchCode)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is InitialWindows main)
                {
                    main.MainFrame.Navigate(new GamePage());
                }
            });
        }

        public void OnCardPlayed(string matchCode, string player, string card) 
        {
            
        }
        public void OnChatMessage(string matchCode, string player, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is InitialWindows main)
                {
                    if (main.MainFrame.Content is LobbyPage lobbyPage)
                    {
                        lobbyPage.AddChatMessage(player, message);
                    }
                    else if (main.MainFrame.Content is GamePage gamePage)
                    {
                        gamePage.ReceiveChatMessage(player, message);
                    }
                }
            });
        }

        public void OnMatchEnded(string matchCode, string winner) 
        {
            
        }

        public void OnFriendRequestReceived(string fromUser)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(string.Format(Lang.NotificactionDialogTextFriendRequest, fromUser), Lang.NotificactionDialogTextFriendRequestTitle, 
                MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public void OnFriendRequestAccepted(string fromUser)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(string.Format(Lang.NotificactionDialogTextFriendAccepted, fromUser), Lang.NotificactionDialogTextFriendAcceptedTitle, 
                MessageBoxButton.OK, MessageBoxImage.Information);
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
