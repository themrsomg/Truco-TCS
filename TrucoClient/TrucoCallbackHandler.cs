using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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
                if (Application.Current.MainWindow is InitialWindows main)
                {
                    if (main.MainFrame.Content is LobbyPage lobby)
                    {
                        lobby.AddChatMessage(string.Empty, string.Format(Lang.CallbacksTextPlayerJoinedLobby, player));
                        lobby.ReloadPlayersDeferred();
                    }
                    else if (main.MainFrame.Content is GamePage gamePage)
                    {
                        gamePage.ReceiveChatMessage(string.Empty, string.Format(Lang.CallbacksTextPlayerJoinedMatch, player));
                    }
                }
            });
        }

        public void OnPlayerLeft(string matchCode, string player)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is InitialWindows main)
                {
                    if (main.MainFrame.Content is LobbyPage lobbyPage)
                    {
                        lobbyPage.AddChatMessage(string.Empty, string.Format(Lang.CallbacksTextPlayerLeftLobby, player));
                        lobbyPage.ReloadPlayersDeferred();
                    }
                    else if (main.MainFrame.Content is GamePage gamePage)
                    {
                        gamePage.ReceiveChatMessage(string.Empty, string.Format(Lang.CallbacksTextPlayerLeftMatch, player));
                    }
                }
            });
        }

        public void OnMatchStarted(string matchCode, PlayerInfo[] players)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is InitialWindows main)
                {
                    var playerList = players?.ToList() ?? new List<PlayerInfo>();
                    main.MainFrame.Navigate(new GamePage(matchCode, playerList));

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
