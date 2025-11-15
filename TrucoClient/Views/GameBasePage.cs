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

        protected string MatchCode;
        protected static string CurrentPlayer => SessionManager.CurrentUsername;

        protected TextBox TxtChatMessage;
        protected Panel ChatMessagesPanel;
        protected TextBlock BlckPlaceholder;
        protected string CurrentTrucoBetState = "None";

        protected abstract TextBlock TbScoreTeam1 { get; }
        protected abstract TextBlock TbScoreTeam2 { get; }
        protected abstract StackPanel PanelPlayerCards { get; }
        protected abstract StackPanel PanelTableCards { get; }
        protected abstract StackPanel PanelBetOptions { get; }
        protected abstract StackPanel PanelEnvidoOptions { get; }
        protected abstract void UpdateEnvidoBetPanelUI(string callerName, string currentBet, int totalPoints, bool needsResponse);
        protected abstract void HideEnvidoBetPanelUI();
        protected abstract void NotifyEnvidoResultUI(string winnerName, int score, int totalPointsAwarded);

        protected ITrucoMatchService MatchClient { get; private set; }

        public GameBasePage()
        {
            this.Unloaded += GameBasePage_Unloaded;
            // ClientManager.SetCallbackHandler(this);
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
                MessageBox.Show(string.Format(Lang.ExceptionTextUnableConnectChat, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GameBasePage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MatchClient.LeaveMatchChat(this.MatchCode, SessionManager.CurrentUsername);
                // ClientManager.SetCallbackHandler(null); ESE CULO SE MERECE TO
            }
            catch (Exception)
            {
            }
        }

        protected abstract void UpdatePlayerHandUI(List<TrucoCard> hand);
        protected abstract void UpdatePlayedCardUI(string playerName, string cardFileName, bool isLastCardOfRound);
        protected abstract void UpdateTurnUI(string nextPlayerName, string currentBetState); protected abstract void UpdateBetPanelUI(string callerName, string currentBet, bool needsResponse);
        protected abstract void HideBetPanelUI();
        protected abstract void ClearTableUI();

        protected void SendPlayCardCommand(string cardFileName)
        {
            try
            {
                MatchClient.PlayCard(MatchCode, cardFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReceiveCards(List<TrucoCard> hand)
        {
            Dispatcher.Invoke(() =>
            {
                this.CurrentTrucoBetState = "None";
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
                MessageBox.Show($"¡Partida Terminada! Ganador: {winner}", "Fin del Juego", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected void ClickBack(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(Lang.GameTextExitGameConfirmation, Lang.GlobalTextConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ClientManager.MatchClient.LeaveMatchChat(this.MatchCode, SessionManager.CurrentUsername);
                    this.NavigationService.Navigate(new MainPage());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorExitingLobby, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
                FontSize = 13 
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

        protected abstract void LoadPlayerAvatars(List<PlayerInfo> players);

        /*
         * Bla, bla, bla, bla, bla, bla
            Ey, yo', yo', yo', yo', yo', yo', yo'
            Yah (la, la, la, la, la, la, la), blow, blow
            (La, la, la, la, la, la, la)

            Diablo', qué safaera
            Tú tiene' un culo cabrón
            Cualquier cosa que te ponga' rompe la carretera (la, la, la, la, la)
            Muévelo, muévelo, muévelo, muévelo (la, la, la, la, la, la, la)

            Qué safaera (la, la, la, la, la)
            Tú tiene' un culo cabrón
            Cualquier cosa que te ponga' rompe la carretera
            Muévelo, muévelo, muévelo, muévelo

            Qué falta de respeto, mami
            ¿Cómo te atreve' a venir sin panty?
            Hoy saliste puesta pa' mí
            Yo que pensaba que venía a dormir

            No, vino ready ya
            Puesta pa' una cepillá'
            Me chupa la lollipop
            Solita se arrodilla, hey

            ¿Cómo te atreve', mami, a venir sin panty?

            Mera, dímelo, DJ Orma
            ¿Qué tú te cree'? Jodío' cabrón, jeje
            Yo Hago Lo Que Me Da La Gana
            Díselo, Conejo
            Ey, ey

            Hoy se bebe, hoy se gasta
            Hoy se fuma como un rasta
            Si Dio' lo permite (si Dio' lo permite), ey
            Si Dio' lo permite (que si Dio' lo permite), ey

            Hoy se bebe, hoy se gasta
            Hoy se fuma como un rasta
            Si Dio' lo permite, ey
            Si Dio' lo permite (yo', yo'), ey

            Real G
            Orientando la' generacione' nueva'
            Con la verdadera

            Bellaqueo a lo galactic
            Sí, pa' que se te mojen los panty
            Métele bellaco a lo versátil
            Má' puta que Betty Boop
            La que se puso bellaca, mami, fuiste tú
            Sigo matando con la U

            Chocha con bicho, bicho con nalga
            Cho-Chocha con bicho, bicho con nalga, sí
            Chocha con bicho, bicho con nalga
            Te-Te está rozando mi tetilla

            Este año no quiero putilla
            Te ven con mucha' prenda' y se quieren pegar
            Te ven bien activa'o y se quieren pegar
            Porque está' bien buena, porque está' bien buena

            Teta' bien grande' como Lourdes Chacón
            Las nalga' bien grande' como Iris Chacón
            La chocha no sé, porque no la he visto
            Pero vamo' pa' la cama a clavarte en panty

            Hoy se bebe, hoy se gasta
            Hoy se fuma como un rasta
            Si Dio' lo permite
            Si Dio' lo permite, yeh-yeh

            Y hoy se bebe, hoy se gasta
            Hoy se fuma como un rasta
            Si Dio' lo permite
            Si Dio' lo permite

            Mami, ¿qué tú quiere'?
            Aquí llegó tu tiburón
            Yo quiero perrearte y fumarme un blunt
            Ver lo que esconde ese pantalón

            Yo quiero perrearte y perrearte y perrearte
            Yo-Yo-Yo quiero perrearte y fumarme un blunt
            Yo quiero perrearte y perrearte y perrear
            Yo-Yo-Yo quiero perrearte y fumarme un blunt, un blunt

            La rola ya me explotó
            La nena bailando se botó
            Ese culo se merece to'
            Se merece to', se merece to', yes
            Ese culo se merece to'
            Se merece to' (ey, ey, ey), se merece to' (ey, ey)

            Ah, yo pensaba que se ponía lenta
            'Tá bien, 'tá bien, vamo' de nuevo, de nuevo
            Miren a Orma, miren a Orma, que está bellaco

            Mi bicho anda fuga'o y yo quiero que tú me lo esconda'
            Agárralo como bonga
            Se metió una pepa que la pone cachonda
            Chinga en lo' Audi, no en lo' Honda, ey

            Si te lo meto, no me llame'
            Que esto no es pa' que me ame', ey
            Si tu novio no te mama el culo
            Pa' eso que no mame

            Baja pa' casa, que yo te lambo to'a
            Mami, yo te lambo to'a
            Baja pa' casa, que yo te rompo to'a, ey
            Que yo te rompo to'a

            Baja pa' casa, que yo te lambo to'a (¡sigue!)
            Mami, yo te lambo to'a (¡sigue!)
            Dime, sierva (papi, sigue)
            Si tú fuma' yerba (papi, pa-papi)

            Jowell
            Bebé, bebé, bebé

            Perreando en la bichota
            Se ve que chinga rico en la nota
            Yo quiero tirarme un selfie con esa' nalgota'
            Para'o, para'o, para'o lo tengo, se me nota
            ¿Qué vamo' a hacer con esa' nalgota'?

            En la uni que son A, A, A
            Pero esa' teta' son C
            Tú ere' una superbellaca, mami, yo lo sé
            Yo también soy un bellaco, ¿qué vamo' a hacer? (Tú sabe', eh)

            Con ese bum-bum, guíllate, bum-bum
            Guíllate ese bum-bum, guíllate, bum-bum
            Si tiene' ese bum-bum, guíllate, bum-bum
            Si tiene' ese bum-bum, guíllate, ¡buoh!
        */
    }
}