using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Services;
using TrucoClient.Properties.Langs;
using TrucoClient.Views;

namespace TrucoClient.TrucoServer
{
    public class TrucoCallbackHandler : ITrucoUserServiceCallback, ITrucoFriendServiceCallback, ITrucoMatchServiceCallback
    {
        public static List<TrucoCard> BufferedHand { get; set; }

        private static IChatPage GetActiveChatPage()
        {
            if (Application.Current.MainWindow is InitialWindows main && main.MainFrame.Content is IChatPage chatPage)
            {
                return chatPage;
            }
            return null;
        }

        private static GameBasePage GetActiveGamePage()
        {
            if (Application.Current.MainWindow is InitialWindows main && main.MainFrame.Content is GameBasePage gamePage)
            {
                return gamePage;
            }
            return null;
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
                    else if (main.MainFrame.Content is IChatPage chatPage)
                    {
                        chatPage.AddChatMessage(string.Empty, string.Format(Lang.CallbacksTextPlayerJoinedMatch, player));
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
                    if (main.MainFrame.Content is LobbyPage lobby)
                    {
                        lobby.AddChatMessage(string.Empty, string.Format(Lang.CallbacksTextPlayerLeftLobby, player));
                        lobby.ReloadPlayersDeferred();
                    }
                    else if (main.MainFrame.Content is IChatPage chatPage)
                    {
                        chatPage.AddChatMessage(string.Empty, string.Format(Lang.CallbacksTextPlayerLeftMatch, player));
                    }
                }
            });
        }

        public void OnMatchStarted(string matchCode, PlayerInfo[] players)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    Console.WriteLine($"[CLIENT] OnMatchStarted for {matchCode}");
                    Console.WriteLine($"[CLIENT] Received {players.Length} players:");

                    for (int i = 0; i < players.Length; i++)
                    {
                        Console.WriteLine($"  [{i}] {players[i].Username} - {players[i].Team}");
                    }

                    if (Application.Current.MainWindow?.Content is Frame frame)
                    {
                        if (frame.Content is LobbyPage lobbyPage)
                        {
                            var orderedPlayers = ClientManager.MatchClient.GetLobbyPlayers(matchCode);

                            Console.WriteLine($"[CLIENT] After GetLobbyPlayers - {orderedPlayers.Length} players:");
                            for (int i = 0; i < orderedPlayers.Length; i++)
                            {
                                Console.WriteLine($"  [{i}] {orderedPlayers[i].Username} - {orderedPlayers[i].Team}");
                            }

                            var playersList = orderedPlayers.ToList();

                            if (playersList.Count == 2)
                            {
                                frame.Navigate(new GameTwoPlayersPage(matchCode, playersList));
                            }
                            else if (playersList.Count == 4)
                            {
                                frame.Navigate(new GameFourPlayersPage(matchCode, playersList));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLIENT ERROR] OnMatchStarted: {ex.Message}");
                    CustomMessageBox.Show($"Error al iniciar partida: {ex.Message}", /////////
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        public void OnMatchEnded(string matchCode, string winner)
        {
            BufferedHand = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                CustomMessageBox.Show(string.Format(Lang.GameTextMatchEnded, winner),
                        Lang.GameTextMatchEndedTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                (Application.Current.MainWindow as InitialWindows)?.MainFrame.Navigate(new MainPage());
            });
        }

        public void OnCardPlayed(string matchCode, string player, string card) 
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.AddChatMessage(string.Empty, "No cards found");
            });
        }

        public void OnChatMessage(string matchCode, string player, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveChatPage()?.AddChatMessage(player, message);
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
                CustomMessageBox.Show("Match Found", "Joining the match...",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public static void PlayerJoined(string username)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.AddChatMessage(string.Empty, string.Format(Lang.CallbacksTextPlayerJoinedMatch, username));
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

        public void NotifyEnvidoCall(string callerName, string betName, bool needsResponse)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.NotifyEnvidoCall(callerName, betName, needsResponse);
            });
        }

        public void NotifyEnvidoResult(string winnerName, int score, int totalPointsAwarded)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.NotifyEnvidoFlorResult(winnerName, score);
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

        public void NotifyFlorCall(string callerName, string betName, bool needsResponse)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetActiveGamePage()?.NotifyFlorCall(callerName, betName, needsResponse);
            });
        }

        public void Ping()
        {
            /*
             * This method is invoked by the server solely to verify the connection vitality.
             * No logic is required inside because the successful return of the call itself
             * acts as the confirmation (ACK) that the client session is active and reachable.
             */
        }
    }
}
