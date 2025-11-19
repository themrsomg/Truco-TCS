using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;

namespace TrucoClient.Views
{
    public abstract class GameBasePage : Page
    {
        protected const string DEFAULT_AVATAR_ID = "avatar_aaa_default";
        protected const string MESSAGE_ERROR = "Error";
        protected const string DEFAULT_AVATAR_PATH = "/Resources/Avatars/avatar_aaa_default.png";
        protected const string DEFAULT_CARD_BACK_PATH = "/Resources/back_card.png";
        private const string BET_NONE = "None";
        private const int MESSAGE_FONT_SIZE = 13;

        protected string MatchCode;
        protected static string CurrentPlayer => SessionManager.CurrentUsername;

        protected TextBox TxtChatMessage;
        protected Panel ChatMessagesPanel;
        protected TextBlock BlckPlaceholder;
        protected string CurrentTrucoBetState = BET_NONE;
        
        protected abstract TextBlock TbScoreTeam1 { get; }
        protected abstract TextBlock TbScoreTeam2 { get; }
        protected abstract StackPanel PanelPlayerCards { get; }
        protected abstract StackPanel PanelTableCards { get; }
        protected abstract StackPanel PanelBetOptions { get; }
        protected abstract StackPanel PanelEnvidoOptions { get; }
        protected abstract void UpdateEnvidoBetPanelUI(string callerName, string currentBet, int totalPoints, bool needsResponse);
        protected abstract void HideEnvidoBetPanelUI();
        protected abstract void NotifyEnvidoResultUI(string winnerName, int score, int totalPointsAwarded);
        protected abstract void LoadPlayerAvatars(List<PlayerInfo> players);
        protected abstract void UpdatePlayerHandUI(List<TrucoCard> hand);
        protected abstract void UpdatePlayedCardUI(string playerName, string cardFileName, bool isLastCardOfRound);
        protected abstract void UpdateTurnUI(string nextPlayerName, string currentBetState); protected abstract void UpdateBetPanelUI(string callerName, string currentBet, bool needsResponse);
        protected abstract void HideBetPanelUI();
        protected abstract void ClearTableUI();
        protected abstract void UpdateFlorBetPanelUI(string callerName, string currentBet, int totalPoints, bool needsResponse);
        protected abstract void HideFlorBetPanelUI();
        protected ITrucoMatchService MatchClient { get; private set; }

        protected GameBasePage()
        {
            this.Unloaded += GameBasePage_Unloaded;
        }

        protected void InitializeBase(string matchCode, TextBox txtChatMessage, Panel chatMessagesPanel, TextBlock blckPlaceholder)
        {
            this.MatchCode = matchCode;
            this.TxtChatMessage = txtChatMessage;
            this.ChatMessagesPanel = chatMessagesPanel;
            this.BlckPlaceholder = blckPlaceholder;

            InitializeMatchClient();
            ConnectToChat();
            if (TrucoCallbackHandler.BufferedHand != null)
            {
                ReceiveCards(TrucoCallbackHandler.BufferedHand);
                TrucoCallbackHandler.BufferedHand = null; 
            }
        }

        private void InitializeMatchClient()
        {
            MatchClient = ClientManager.MatchClient;
        }

        private void ConnectToChat()
        {
            try
            {
                var matchClient = ClientManager.MatchClient;
                matchClient.JoinMatchChat(this.MatchCode, SessionManager.CurrentUsername);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextUnableConnectChat, ex.Message), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GameBasePage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MatchClient.LeaveMatchChat(this.MatchCode, SessionManager.CurrentUsername);
            }
            catch (Exception)
            {
                /* 
                 * Any exceptions (e.g., CommunicationException or TimeoutException) 
                 * that may occur when attempting to leave the WCF chat are intentionally ignored.
                 * This is common when the channel has already been closed or is in 
                 * a 'Faulted' state upon page loading.
                 */
            }
        }

        protected void SendPlayCardCommand(string cardFileName)
        {
            try
            {
                MatchClient.PlayCard(MatchCode, cardFileName);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void SendCallTrucoCommand(string betType)
        {
            try
            {
                MatchClient.CallTruco(MatchCode, betType);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void SendResponseCommand(string response)
        {
            try
            {
                MatchClient.RespondToCall(MatchCode, response);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReceiveCards(List<TrucoCard> hand)
        {
            Dispatcher.Invoke(() =>
            {
                this.CurrentTrucoBetState = BET_NONE;
                UpdatePlayerHandUI(hand);
            });
        }

        public void NotifyCardPlayed(string playerName, string cardFileName, bool isLastCardOfRound)
        {
            Dispatcher.Invoke(() => UpdatePlayedCardUI(playerName, cardFileName, isLastCardOfRound));
        }

        public void NotifyTurnChange(string nextPlayerName)
        {
            Dispatcher.Invoke(() => UpdateTurnUI(nextPlayerName, this.CurrentTrucoBetState)); 
        }

        public void NotifyScoreUpdate(int team1Score, int team2Score)
        {
            Dispatcher.Invoke(() =>
            {
                TbScoreTeam1.Text = team1Score.ToString();
                TbScoreTeam2.Text = team2Score.ToString();
            });
        }

        public void NotifyTrucoCall(string callerName, string betName, bool needsResponse)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateBetPanelUI(callerName, betName, needsResponse);
                AddChatMessage(null, $"{callerName} cantó {betName}");
            });
        }

        public void NotifyRoundEnd(string winnerName, int team1Score, int team2Score)
        {
            Dispatcher.Invoke(() =>
            {
                AddChatMessage(null, $"Ronda ganada por: {winnerName}");
                TbScoreTeam1.Text = team1Score.ToString();
                TbScoreTeam2.Text = team2Score.ToString();

                ClearTableUI();
            });
        }

        public void OnChatMessage(string matchCode, string player, string message)
        {
            Dispatcher.Invoke(() => AddChatMessage(player, message));
        }

        public void OnMatchEnded(string matchCode, string winner)
        {
            Dispatcher.Invoke(() =>
            {
                CustomMessageBox.Show(string.Format(Lang.GameTextMatchEnded, winner), 
                    Lang.GameTextMatchEndedTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                this.NavigationService.Navigate(new MainPage());
            });
        }

        protected void SendCallEnvidoCommand(string betType)
        {
            try
            {
                MatchClient.CallEnvido(MatchCode, betType);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void SendCallFlorCommand(string betType)
        {
            try
            {
                MatchClient.CallFlor(MatchCode, betType);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NotifyFlorCall(string callerName, string betName, int totalPoints, bool needsResponse)
        {
            Dispatcher.Invoke(() =>
            {
                HideBetPanelUI();
                HideEnvidoBetPanelUI();

                UpdateFlorBetPanelUI(callerName, betName, totalPoints, needsResponse);
                AddChatMessage(null, $"{callerName} cantó {betName} ({totalPoints} puntos en juego)");
            });
        }

        protected void SendRespondToEnvidoCommand(string response)
        {
            try
            {
                MatchClient.RespondToEnvido(MatchCode, response);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NotifyEnvidoCall(string callerName, string betName, int totalPoints, bool needsResponse)
        {
            Dispatcher.Invoke(() =>
            {
                HideBetPanelUI();
                UpdateEnvidoBetPanelUI(callerName, betName, totalPoints, needsResponse);
                AddChatMessage(null, $"{callerName} cantó {betName} ({totalPoints} puntos en juego)");
            });
        }

        public void NotifyEnvidoResult(string winnerName, int score, int totalPointsAwarded)
        {
            Dispatcher.Invoke(() =>
            {
                HideEnvidoBetPanelUI();
                AddChatMessage(null, $"Envido ganado por: {winnerName} con {score}. ({totalPointsAwarded} puntos)");
            });
        }

        public void NotifyResponse(string responderName, string response, string newBetState)
        {
            Dispatcher.Invoke(() =>
            {
                this.CurrentTrucoBetState = newBetState;
                HideBetPanelUI();
                HideEnvidoBetPanelUI();
                AddChatMessage(null, $"{responderName} dijo: {response}");
            });
        }

        protected void SendRespondToFlorCommand(string response)
        {
            try
            {
                MatchClient.RespondToEnvido(MatchCode, response);
            }
            catch (Exception ex) { /* ... */ }
        }

        protected void ClickSendMessage(object sender, RoutedEventArgs e)
        {
            string messageText = TxtChatMessage.Text.Trim();
            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }

            AddChatMessage(Lang.ChatTextYou, messageText);
            TxtChatMessage.Clear();

            try
            {
                ClientManager.MatchClient.SendChatMessage(this.MatchCode, CurrentPlayer, messageText);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void ClickOpenGesturesMenu(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }

        protected void ClickGesture(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                string emoji = item.Header.ToString();
                AddChatMessage(Lang.ChatTextYou, emoji);
                try
                {
                    ClientManager.MatchClient.SendChatMessage(this.MatchCode, CurrentPlayer, emoji);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), 
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected void ClickBack(object sender, RoutedEventArgs e)
        {
            bool? result = CustomMessageBox.Show(Lang.GameTextExitGameConfirmation, Lang.GlobalTextConfirmation, 
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == true)
            {
                try
                {
                    ClientManager.MatchClient.LeaveMatchChat(this.MatchCode, SessionManager.CurrentUsername);
                    this.NavigationService.Navigate(new MainPage());
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(string.Format(Lang.ExceptionTextErrorExitingLobby, ex.Message), 
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender == TxtChatMessage)
            {
                ClickSendMessage(sender, null);
                e.Handled = true;
            }
        }

        protected void AddChatMessage(string senderName, string message)
        {
            Border messageBubble = new Border 
            { 
                Padding = new Thickness(5), 
                Margin = new Thickness(2)
            };
            TextBlock messageText = new TextBlock 
            { 
                TextWrapping = TextWrapping.Wrap, 
                FontSize = MESSAGE_FONT_SIZE 
            };

            if (string.IsNullOrEmpty(senderName))
            {
                messageText.Text = message;
                messageText.Foreground = Brushes.DarkGray;
                messageText.FontStyle = FontStyles.Italic;
            }
            else
            {
                messageText.Text = $"{senderName}: {message}";
            }

            messageBubble.Child = messageText;
            ChatMessagesPanel.Children.Add(messageBubble);

            if (VisualTreeHelper.GetParent(ChatMessagesPanel) is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToBottom();
            }
        }

        protected void ChatMessageTextChanged(object sender, TextChangedEventArgs e)
        {
            if (BlckPlaceholder != null && TxtChatMessage != null)
            {
                BlckPlaceholder.Visibility = string.IsNullOrEmpty(TxtChatMessage.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public void ReceiveChatMessage(string senderName, string message)
        {
            AddChatMessage(senderName, message);
        }

        protected static BitmapImage LoadAvatar(string avatarId)
        {
            if (string.IsNullOrWhiteSpace(avatarId))
            {
                avatarId = DEFAULT_AVATAR_ID;
            }
            try
            {
                return new BitmapImage(new Uri($"/Resources/Avatars/{avatarId}.png", UriKind.Relative));
            }
            catch
            {
                return new BitmapImage(new Uri(DEFAULT_AVATAR_PATH, UriKind.Relative));
            }
        }

        protected static BitmapImage LoadCardImage(string cardFileName)
        {
            if (string.IsNullOrWhiteSpace(cardFileName))
            {
                return new BitmapImage(new Uri(DEFAULT_CARD_BACK_PATH, UriKind.Relative));
            }
            try
            {
                return new BitmapImage(new Uri($"/Resources/Cards/{cardFileName}.png", UriKind.Relative));
            }
            catch
            {
                return new BitmapImage(new Uri(DEFAULT_CARD_BACK_PATH, UriKind.Relative));
            }
        }

        protected void SendGoToDeckCommand()
        {
            try
            {
                MatchClient.GoToDeck(MatchCode);
            }
            catch (Exception ex)
            {
                // TODO: Cambiar esto
                CustomMessageBox.Show(string.Format("Esto se debe cambiar", ex.Message),
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}