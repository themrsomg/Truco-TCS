using System;
using System.Collections.Generic;
using System.Linq;
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
    public abstract class GameBasePage : Page, IChatPage
    {
        protected const string DEFAULT_AVATAR_ID = "avatar_aaa_default";
        protected const string MESSAGE_ERROR = "Error";
        protected const string DEFAULT_AVATAR_PATH = "/Resources/Avatars/avatar_aaa_default.png";
        protected const string DEFAULT_CARD_BACK_PATH = "/Resources/back_card.png";
        private const string BET_NONE = "None";
        private const int MESSAGE_FONT_SIZE = 13;

        protected const string BET_STATUS_NONE = "None";
        protected const string BET_TRUCO = "Truco";
        protected const string BET_RETRUCO = "Retruco";
        protected const string BET_VALE_CUATRO = "ValeCuatro";
        protected const string RESPOND_QUIERO = "Quiero";
        protected const string RESPOND_NO_QUIERO = "NoQuiero";
        protected const string BET_ENVIDO = "Envido";
        protected const string BET_REAL_ENVIDO = "RealEnvido";
        protected const string BET_FALTA_ENVIDO = "FaltaEnvido";
        protected const string BET_FLOR = "Flor";
        protected const string BET_CONTRA_FLOR = "ContraFlor";

        protected string MatchCode;
        protected static string currentPlayer => SessionManager.CurrentUsername;
        protected TextBox txtChatMessage;
        protected Panel chatMessagesPanel;
        protected TextBlock blckPlaceholder;
        protected string currentTrucoBetState = BET_NONE;
        private bool florPlayedInCurrentHand = false;

        protected List<TrucoCard> playerHand = new List<TrucoCard>();

        protected abstract StackPanel PanelBetOptions { get; }
        protected abstract StackPanel PanelEnvidoOptions { get; }
        protected abstract StackPanel PanelFlorOptions { get; }
        protected abstract StackPanel PanelPlayerCards { get; }
        protected abstract StackPanel PanelTableCards { get; }
        protected abstract TextBlock TxtScoreTeam1 { get; }
        protected abstract TextBlock TxtScoreTeam2 { get; }

        protected abstract TextBlock TxtTrucoCaller { get; }
        protected abstract TextBlock TxtEnvidoCaller { get; }
        protected abstract TextBlock TxtFlorCaller { get; }

        protected abstract Button BtnCallTruco { get; }
        protected abstract Button BtnRespondQuiero { get; }
        protected abstract Button BtnRespondNoQuiero { get; }
        protected abstract Button BtnGoToDeck { get; }

        protected abstract Button BtnCallEnvido { get; }
        protected abstract Button BtnCallRealEnvido { get; }
        protected abstract Button BtnCallFaltaEnvido { get; }
        protected abstract Button BtnEnvidoRespondQuiero { get; }
        protected abstract Button BtnEnvidoRespondNoQuiero { get; }

        protected abstract Button BtnStartFlor { get; }
        protected abstract Button BtnCallFlor { get; }
        protected abstract Button BtnCallContraFlor { get; }

        protected abstract void LoadPlayerAvatars(List<PlayerInfo> players);
        protected abstract void UpdatePlayerHandUI(List<TrucoCard> hand);
        protected abstract void UpdateTurnUI(string nextPlayerName, string currentBetState);

        protected ITrucoMatchService MatchClient { get; private set; }

        protected GameBasePage()
        {
            this.Unloaded += GameBasePage_Unloaded;
        }

        protected void InitializeBase(string matchCode, TextBox txtChatMessage, Panel chatMessagesPanel, TextBlock blckPlaceholder)
        {
            this.MatchCode = matchCode;
            this.txtChatMessage = txtChatMessage;
            this.chatMessagesPanel = chatMessagesPanel;
            this.blckPlaceholder = blckPlaceholder;

            InitializeMatchClient();
            ConnectToChat();
        }

        protected void CheckForBufferedCards()
        {
            if (TrucoCallbackHandler.BufferedHand != null)
            {
                ReceiveCards(TrucoCallbackHandler.BufferedHand);
                TrucoCallbackHandler.BufferedHand = null;
            }
        }

        private void InitializeMatchClient() => MatchClient = ClientManager.MatchClient;

        private void ConnectToChat()
        {
            try
            {
                ClientManager.MatchClient.JoinMatchChat(this.MatchCode, SessionManager.CurrentUsername);
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
            } catch 
            {
                /* error */
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
                ShowError(ex); 
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
                ShowError(ex); 
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
                ShowError(ex); 
            }
        }

        protected void SendCallEnvidoCommand(string betType)
        {
            try 
            { 
                MatchClient.CallEnvido(MatchCode, betType); 
            }
            catch (Exception ex) 
            { 
                ShowError(ex); 
            }
        }

        protected void SendCallFlorCommand(string betType)
        {
            try 
            {
                florPlayedInCurrentHand = true;
                BtnStartFlor.Visibility = Visibility.Collapsed;

                MatchClient.CallFlor(MatchCode, betType); 
            }
            catch (Exception ex) 
            { 
                ShowError(ex); 
            }
        }

        protected void SendRespondToEnvidoCommand(string response)
        {
            try 
            { 
                MatchClient.RespondToEnvido(MatchCode, response); 
            }
            catch (Exception ex) 
            { 
                ShowError(ex); 
            }
        }

        protected void SendRespondToFlorCommand(string response)
        {
            try 
            { 
                MatchClient.RespondToFlor(MatchCode, response); 
            }
            catch (Exception ex) 
            { 
                ShowError(ex); 
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
                ShowError(ex); 
            }
        }

        private void ShowError(Exception ex)
        {
            CustomMessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message),
                MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ReceiveCards(List<TrucoCard> hand)
        {
            Dispatcher.Invoke(() =>
            {
                this.currentTrucoBetState = BET_NONE;
                this.playerHand = hand;
                this.florPlayedInCurrentHand = false;
                UpdatePlayerHandUI(hand);
            });
        }

        public void NotifyCardPlayed(string playerName, string cardFileName, bool isLastCardOfRound)
        {
            Dispatcher.Invoke(() => UpdatePlayedCardUI(playerName, cardFileName, isLastCardOfRound));
        }

        protected virtual void UpdatePlayedCardUI(string playerName, string cardFileName, bool isLastCardOfRound)
        {
            Image cardImage = new Image
            {
                Source = LoadCardImage(cardFileName),
                Width = 100,
                Height = 150,
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Top
            };
            PanelTableCards.Children.Add(cardImage);
        }

        public void NotifyTurnChange(string nextPlayerName)
        {
            Dispatcher.Invoke(() =>
            {
                HideFlorBetPanelUI();
                UpdateTurnUI(nextPlayerName, this.currentTrucoBetState);
            });
        }

        protected void UpdateTurnButtons(bool isMyTurn, string currentBetState)
        {
            if (isMyTurn)
            {
                PanelBetOptions.Visibility = Visibility.Visible;
                TxtTrucoCaller.Visibility = Visibility.Collapsed;
                BtnRespondQuiero.Visibility = Visibility.Collapsed;
                BtnRespondNoQuiero.Visibility = Visibility.Collapsed;

                BtnCallTruco.Visibility = Visibility.Visible;
                BtnGoToDeck.Visibility = Visibility.Visible;

                bool handJustStarted = PanelTableCards.Children.Count == 0;

                if (currentBetState == BET_STATUS_NONE && ClientHasFlor() && handJustStarted && !florPlayedInCurrentHand)
                {
                    BtnStartFlor.Visibility = Visibility.Visible;
                }
                else
                {
                    BtnStartFlor.Visibility = Visibility.Collapsed;
                }

                if (currentBetState == BET_STATUS_NONE)
                {
                    BtnCallTruco.Content = BET_TRUCO;
                }
                else if (currentBetState == BET_TRUCO)
                {
                    BtnCallTruco.Content = BET_RETRUCO;
                }
                else if (currentBetState == BET_RETRUCO)
                {
                    BtnCallTruco.Content = BET_VALE_CUATRO;
                }
                else
                {
                    BtnCallTruco.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                PanelBetOptions.Visibility = Visibility.Collapsed;
                BtnGoToDeck.Visibility = Visibility.Collapsed;
            }
        }

        public void NotifyScoreUpdate(int team1Score, int team2Score)
        {
            Dispatcher.Invoke(() =>
            {
                TxtScoreTeam1.Text = team1Score.ToString();
                TxtScoreTeam2.Text = team2Score.ToString();
            });
        }

        public void NotifyTrucoCall(string callerName, string betName, bool needsResponse)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateBetPanelUI(callerName, betName, needsResponse);
                AddChatMessage(null, string.Format(Lang.GameTextPlayerCalledBet, callerName, betName));
            });
        }

        protected void UpdateBetPanelUI(string callerName, string currentBet, bool needsResponse)
        {
            if (needsResponse)
            {
                PanelBetOptions.Visibility = Visibility.Visible;
                TxtTrucoCaller.Text = string.Format(Lang.GameTextPlayerCalledBet, callerName, currentBet);
                TxtTrucoCaller.Visibility = Visibility.Visible;
                BtnCallTruco.Visibility = Visibility.Collapsed;
                BtnStartFlor.Visibility = Visibility.Collapsed;
                BtnRespondQuiero.Visibility = Visibility.Visible;
                BtnRespondNoQuiero.Visibility = Visibility.Visible;
            }
            else
            {
                TxtTrucoCaller.Visibility = Visibility.Collapsed;
                PanelBetOptions.Visibility = Visibility.Collapsed;
            }
        }

        protected void HideBetPanelUI() => PanelBetOptions.Visibility = Visibility.Collapsed;

        public void NotifyRoundEnd(string winnerName, int team1Score, int team2Score)
        {
            Dispatcher.Invoke(() =>
            {
                AddChatMessage(null, string.Format(Lang.GameTextRoundWonBy, winnerName));
                TxtScoreTeam1.Text = team1Score.ToString();
                TxtScoreTeam2.Text = team2Score.ToString();
                ClearTableUI();
            });
        }

        protected void ClearTableUI() => PanelTableCards.Children.Clear();

        public void NotifyEnvidoCall(string callerName, string betName, bool needsResponse)
        {
            Dispatcher.Invoke(() =>
            {
                HideBetPanelUI();
                UpdateEnvidoBetPanelUI(callerName, betName, needsResponse);
                AddChatMessage(null, string.Format(Lang.GameTextPlayerCalledBet, callerName, betName));
            });
        }

        protected void UpdateEnvidoBetPanelUI(string callerName, string currentBet, bool needsResponse)
        {
            PanelEnvidoOptions.Visibility = Visibility.Visible;
            TxtEnvidoCaller.Text = string.Format(Lang.GameTextPlayerCalledBet, callerName, currentBet);

            bool isMyTurnToRespond = needsResponse && (callerName != currentPlayer);

            BtnEnvidoRespondQuiero.Visibility = isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
            BtnEnvidoRespondNoQuiero.Visibility = isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;

            BtnCallEnvido.Visibility = !isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
            BtnCallRealEnvido.Visibility = !isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
            BtnCallFaltaEnvido.Visibility = !isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
        }

        protected void HideEnvidoBetPanelUI() => PanelEnvidoOptions.Visibility = Visibility.Collapsed;

        public void NotifyEnvidoFlorResult(string winnerName, int score)
        {
            Dispatcher.Invoke(() =>
            {
                HideEnvidoBetPanelUI();
                HideFlorBetPanelUI();
                AddChatMessage(null, string.Format(Lang.GameTextBetWonBy, winnerName, score));
            });
        }

        public void NotifyFlorCall(string callerName, string betName, int totalPoints, bool needsResponse)
        {
            Dispatcher.Invoke(() =>
            {
                florPlayedInCurrentHand = true;

                HideBetPanelUI();
                HideEnvidoBetPanelUI();

                if (!needsResponse)
                {
                    AddChatMessage(null, string.Format(Lang.GameTextPlayerHasFlor, callerName, callerName));
                    return;
                }

                UpdateFlorBetPanelUI(callerName, betName, needsResponse);
                AddChatMessage(null, string.Format(Lang.GameTextPlayerCalledBet, callerName, betName));
            });
        }

        protected void UpdateFlorBetPanelUI(string callerName, string currentBet, bool needsResponse)
        {
            PanelFlorOptions.Visibility = Visibility.Visible;
            TxtFlorCaller.Text = string.Format(Lang.GameTextPlayerCalledBet, callerName, currentBet);

            bool isMyTurnToRespond = needsResponse && (callerName != currentPlayer);

            BtnStartFlor.Visibility = Visibility.Collapsed;
            BtnCallFlor.Visibility = Visibility.Collapsed;

            if (isMyTurnToRespond)
            {
                BtnCallContraFlor.Visibility = Visibility.Visible;
            }
            else
            {
                BtnCallContraFlor.Visibility = Visibility.Collapsed;
                TxtFlorCaller.Text += Lang.GameTextWaitingResponse;
            }
        }

        protected void HideFlorBetPanelUI() => PanelFlorOptions.Visibility = Visibility.Collapsed;

        public void NotifyResponse(string responderName, string response, string newBetState)
        {
            Dispatcher.Invoke(() =>
            {
                this.currentTrucoBetState = newBetState;
                HideBetPanelUI();
                HideEnvidoBetPanelUI();
                HideFlorBetPanelUI();
                AddChatMessage(null, string.Format(Lang.GameTextPlayerSaidResponse, responderName, response));
            });
        }

        protected void SetupCommonEventHandlers(Button btnBack, Button btnTruco, Button btnQuiero, Button btnNoQuiero)
        {
            if (btnBack != null)
            {
                btnBack.Click -= ClickBack;
                btnBack.Click += ClickBack;
            }

            if (btnQuiero != null)
            {
                btnQuiero.Click += (s, e) => SendResponseCommand(RESPOND_QUIERO);
            }
            if (btnNoQuiero != null)
            {
                btnNoQuiero.Click += (s, e) => SendResponseCommand(RESPOND_NO_QUIERO);
            }

            if (btnTruco != null)
            {
                btnTruco.Click += (s, e) =>
                {
                    string betToSend = btnTruco.Content.ToString();
                    SendCallTrucoCommand(betToSend);
                };
            }
        }

        protected bool ClientHasFlor()
        {
            if (this.playerHand == null || this.playerHand.Count < 3)
            {
                return false;
            }

            return this.playerHand.GroupBy(card => card.CardSuit).Any(g => g.Count() >= 3);
        }

        protected void ClickBack(object sender, RoutedEventArgs e)
        {
            bool? result = CustomMessageBox.Show(Lang.GameTextExitGameConfirmation, Lang.GlobalTextConfirmation, 
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == true)
            {
                try 
                { 
                    MatchClient.LeaveMatchChat(this.MatchCode, SessionManager.CurrentUsername); 
                }
                catch (Exception ex) 
                { 
                    Console.WriteLine(ex.Message); 
                }
                finally 
                { 
                    this.NavigationService.Navigate(new MainPage()); 
                }
            }
        }

        protected void ClickSendMessage(object sender, RoutedEventArgs e)
        {
            string messageText = txtChatMessage.Text.Trim();
            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }

            AddChatMessage(Lang.ChatTextYou, messageText);
            txtChatMessage.Clear();
            try 
            { 
                ClientManager.MatchClient.SendChatMessage(this.MatchCode, currentPlayer, messageText); 
            }
            catch (Exception ex) 
            { 
                ShowError(ex); 
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
                    ClientManager.MatchClient.SendChatMessage(this.MatchCode, currentPlayer, emoji); 
                }
                catch (Exception ex) 
                { 
                    ShowError(ex); 
                }
            }
        }

        protected void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender == txtChatMessage)
            {
                ClickSendMessage(sender, null);
                e.Handled = true;
            }
        }

        public void AddChatMessage(string senderName, string message)
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
            chatMessagesPanel.Children.Add(messageBubble);

            if (VisualTreeHelper.GetParent(chatMessagesPanel) is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToBottom();
            }
        }

        protected void ChatMessageTextChanged(object sender, TextChangedEventArgs e)
        {
            if (blckPlaceholder != null && txtChatMessage != null)
            {
                blckPlaceholder.Visibility = string.IsNullOrEmpty(txtChatMessage.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
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
    }
}