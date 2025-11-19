using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TrucoClient.Helpers.Services;
using TrucoClient.Properties.Langs;
using TrucoClient.Views;

namespace TrucoClient.TrucoServer
{
    public class TrucoCallbackHandler : ITrucoUserServiceCallback, ITrucoFriendServiceCallback, ITrucoMatchServiceCallback
    {
        private const string MESSAGE_ERROR = "Error";
        public static List<TrucoCard> BufferedHand { get; set; }

        private static GameBasePage GetActiveGamePage()
        {
            if (Application.Current.MainWindow is InitialWindows main && main.MainFrame.Content is GameBasePage gamePage)
            {
                return gamePage;
            }
            return null;
        }

        public void NotifyEnvidoCall(string callerName, string betName, int totalPoints, bool needsResponse)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.NotifyEnvidoCall(callerName, betName, totalPoints, needsResponse);
            });
        }

        public void NotifyEnvidoResult(string winnerName, int score, int totalPointsAwarded)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.NotifyEnvidoResult(winnerName, score, totalPointsAwarded);
            });
        }

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
                    else if (main.MainFrame.Content is GameBasePage gamePage)
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
                    else if (main.MainFrame.Content is GameBasePage gamePage)
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

                    if (playerList.Count == 2)
                    {
                        main.MainFrame.Navigate(new GameTwoPlayersPage(matchCode, playerList));
                    }
                    else if (playerList.Count == 4)
                    {
                        main.MainFrame.Navigate(new GameFourPlayersPage(matchCode, playerList));
                    }
                    else
                    {
                        CustomMessageBox.Show(Lang.ExceptionTextErrorStartingMatch, MESSAGE_ERROR, 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            });
        }

        public void OnCardPlayed(string matchCode, string player, string card) 
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.ReceiveChatMessage(string.Empty, string.Format("No hay cartas encontradas", player, card));
            });
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
                    else if (main.MainFrame.Content is GameBasePage gamePage)
                    {
                        gamePage.ReceiveChatMessage(player, message);
                    }
                }
            });
        }

        public void OnMatchEnded(string matchCode, string winner) 
        {
            BufferedHand = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                var gamePage = GetActiveGamePage();

                if (gamePage != null)
                {
                    CustomMessageBox.Show(string.Format(Lang.GameTextMatchEnded, winner),
                        Lang.GameTextMatchEndedTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                    (Application.Current.MainWindow as InitialWindows)?.MainFrame.Navigate(new MainPage());
                }
                else
                {
                    MessageBox.Show(string.Format("Fin", matchCode, winner), "La partida ha terminado",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
        }

        public void OnFriendRequestReceived(string fromUser)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CustomMessageBox.Show(string.Format(Lang.NotificactionDialogTextFriendRequest, fromUser), 
                    Lang.NotificactionDialogTextFriendRequestTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public void OnFriendRequestAccepted(string fromUser)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CustomMessageBox.Show(string.Format(Lang.NotificactionDialogTextFriendAccepted, fromUser),
                    Lang.NotificactionDialogTextFriendAcceptedTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public static void MatchFound(string matchDetails)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(string.Format("Juego encontrado", matchDetails), "Uniendote a la partida...",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public void PlayerJoined(string username)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.ReceiveChatMessage(string.Empty, string.Format(Lang.CallbacksTextPlayerJoinedMatch, username));
            });
        }

        public void ReceiveCards(TrucoCard[] hand) 
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var gamePage = GetActiveGamePage();
                var handAsList = hand.ToList(); 

                if (gamePage != null)
                {
                    gamePage.ReceiveCards(handAsList);
                    BufferedHand = null;
                }
                else
                {
                    BufferedHand = handAsList;
                }
            });
        }

        public void NotifyCardPlayed(string playerName, string cardFileName, bool isLastCardOfRound)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.NotifyCardPlayed(playerName, cardFileName, isLastCardOfRound);
            });
        }

        public void NotifyTurnChange(string nextPlayerName)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.NotifyTurnChange(nextPlayerName);
            });
        }

        public void NotifyScoreUpdate(int team1Score, int team2Score)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.NotifyScoreUpdate(team1Score, team2Score);
            });
        }

        public void NotifyTrucoCall(string callerName, string betName, bool needsResponse)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.NotifyTrucoCall(callerName, betName, needsResponse);
            });
        }

        public void NotifyResponse(string responderName, string response, string newBetState)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.NotifyResponse(responderName, response, newBetState);
            });
        }

        public void NotifyRoundEnd(string winnerName, int team1Score, int team2Score)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.NotifyRoundEnd(winnerName, team1Score, team2Score);
            });
        }
    }
}
