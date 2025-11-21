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
using System.ServiceModel;
using System.Threading.Tasks;
using System.IO;

namespace TrucoClient.Views
{
    public abstract class GameBasePage : Page, IChatPage
    {

        private const int MESSAGE_FONT_SIZE = 13;
        protected const double OPACITY_ACTIVE = 1.0;
        protected const double OPACITY_INACTIVE = 0.5;

        protected const string DEFAULT_AVATAR_ID = "avatar_aaa_default";
        protected const string MESSAGE_ERROR = "Error";
        protected const string DEFAULT_AVATAR_PATH = "/Resources/Avatars/avatar_aaa_default.png";
        protected const string DEFAULT_CARD_BACK_PATH = "/Resources/back_card.png";
        private const string BET_NONE = "None";

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

        protected Image[] PlayerCardImages { get; set; }
        protected Button BtnBack { get; set; }
        protected StackPanel PanelBetOptions { get; set; }
        protected StackPanel PanelEnvidoOptions { get; set; }
        protected StackPanel PanelFlorOptions { get; set; }
        protected StackPanel PanelPlayerCards { get; set; }
        protected StackPanel PanelTableCards { get; set; }

        protected TextBlock TxtScoreTeam1 { get; set; }
        protected TextBlock TxtScoreTeam2 { get; set; }
        protected TextBlock TxtTrucoCaller { get; set; }
        protected TextBlock TxtEnvidoCaller { get; set; }
        protected TextBlock TxtFlorCaller { get; set; }

        protected Button BtnCallTruco { get; set; }
        protected Button BtnRespondQuiero { get; set; }
        protected Button BtnRespondNoQuiero { get; set; }
        protected Button BtnGoToDeck { get; set; }

        protected Button BtnCallEnvido { get; set; }
        protected Button BtnCallRealEnvido { get; set; }
        protected Button BtnCallFaltaEnvido { get; set; }
        protected Button BtnEnvidoRespondQuiero { get; set; }
        protected Button BtnEnvidoRespondNoQuiero { get; set; }

        protected Button BtnStartFlor { get; set; }
        protected Button BtnCallFlor { get; set; }
        protected Button BtnCallContraFlor { get; set; }

        protected abstract void LoadPlayerAvatars(List<PlayerInfo> players);
        
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

        protected void InitializeGameEvents()
        {
            if (BtnBack != null)
            {
                BtnBack.Click -= ClickBack;
                BtnBack.Click += ClickBack;
            }

            if (BtnCallTruco != null)
            {
                BtnCallTruco.Click -= OnBtnCallTrucoClick;
                BtnCallTruco.Click += OnBtnCallTrucoClick;
            }

            if (BtnRespondQuiero != null)
            {
                BtnRespondQuiero.Click -= OnBtnRespondQuieroClick;
                BtnRespondQuiero.Click += OnBtnRespondQuieroClick;
            }
            if (BtnRespondNoQuiero != null)
            {
                BtnRespondNoQuiero.Click -= OnBtnRespondNoQuieroClick;
                BtnRespondNoQuiero.Click += OnBtnRespondNoQuieroClick;
            }

            if (BtnGoToDeck != null)
            {
                BtnGoToDeck.Click -= OnBtnGoToDeckClick;
                BtnGoToDeck.Click += OnBtnGoToDeckClick;
            }

            if (BtnCallEnvido != null) 
            { 
                BtnCallEnvido.Click -= OnBtnCallEnvidoClick; BtnCallEnvido.Click += OnBtnCallEnvidoClick; 
            }
            if (BtnCallRealEnvido != null) 
            { 
                BtnCallRealEnvido.Click -= OnBtnCallRealEnvidoClick; BtnCallRealEnvido.Click += OnBtnCallRealEnvidoClick; 
            }
            if (BtnCallFaltaEnvido != null) 
            { 
                BtnCallFaltaEnvido.Click -= OnBtnCallFaltaEnvidoClick; BtnCallFaltaEnvido.Click += OnBtnCallFaltaEnvidoClick; 
            }
            if (BtnEnvidoRespondQuiero != null) 
            { 
                BtnEnvidoRespondQuiero.Click -= OnBtnRespondQuieroClick; BtnEnvidoRespondQuiero.Click += OnBtnRespondQuieroClick; 
            }
            if (BtnEnvidoRespondNoQuiero != null) 
            { 
                BtnEnvidoRespondNoQuiero.Click -= OnBtnRespondNoQuieroClick; BtnEnvidoRespondNoQuiero.Click += OnBtnRespondNoQuieroClick; 
            }

            if (BtnStartFlor != null) 
            { 
                BtnStartFlor.Click -= OnBtnCallFlorClick; BtnStartFlor.Click += OnBtnCallFlorClick; 
            }
            if (BtnCallFlor != null) 
            { 
                BtnCallFlor.Click -= OnBtnCallFlorClick; BtnCallFlor.Click += OnBtnCallFlorClick; 
            }
            if (BtnCallContraFlor != null) 
            { 
                BtnCallContraFlor.Click -= OnBtnCallContraFlorClick; BtnCallContraFlor.Click += OnBtnCallContraFlorClick; 
            }
        }


        private void OnBtnCallTrucoClick(object sender, RoutedEventArgs e)
        {
            string betToSend = BtnCallTruco.Content.ToString();
            SendCallTrucoCommand(betToSend);
        }

        private void OnBtnRespondQuieroClick(object sender, RoutedEventArgs e)
        {
            string response = (CurrentStateIsEnvidoOrFlor()) ? RESPOND_QUIERO : RESPOND_QUIERO;

            if (sender == BtnEnvidoRespondQuiero)
            {
                SendRespondToEnvidoCommand(RESPOND_QUIERO);
            }
            else
            {
                SendResponseCommand(RESPOND_QUIERO);
            }
        }

        private void OnBtnRespondNoQuieroClick(object sender, RoutedEventArgs e)
        {
            if (sender == BtnEnvidoRespondNoQuiero)
            {
                SendRespondToEnvidoCommand(RESPOND_NO_QUIERO);
            }
            else
            {
                SendResponseCommand(RESPOND_NO_QUIERO);
            }
        }

        private void OnBtnGoToDeckClick(object sender, RoutedEventArgs e)
        {
            SendGoToDeckCommand();
        }
        private void OnBtnCallEnvidoClick(object sender, RoutedEventArgs e)
        {
            SendCallEnvidoCommand(BET_ENVIDO);
        }
        private void OnBtnCallRealEnvidoClick(object sender, RoutedEventArgs e)
        {
            SendCallEnvidoCommand(BET_REAL_ENVIDO);
        }
        private void OnBtnCallFaltaEnvidoClick(object sender, RoutedEventArgs e)
        {
            SendCallEnvidoCommand(BET_FALTA_ENVIDO);
        }
        private void OnBtnCallFlorClick(object sender, RoutedEventArgs e)
        {
            SendCallFlorCommand(BET_FLOR);
        }
        private void OnBtnCallContraFlorClick(object sender, RoutedEventArgs e)
        {
            SendRespondToFlorCommand(BET_CONTRA_FLOR);
        }

        private bool CurrentStateIsEnvidoOrFlor()
        {
            return false;
        }

        protected void InitializeCardEvents()
        {
            if (PlayerCardImages == null)
            {
                return;
            }

            foreach (var img in PlayerCardImages)
            {
                img.MouseDown -= PlayerCard_MouseDown;
                img.MouseDown += PlayerCard_MouseDown;
            }
        }

        private void InitializeMatchClient()
        {
            try
            {
                MatchClient = ClientManager.MatchClient;
            }
            catch (EndpointNotFoundException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTimeoutChat,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextInvalid,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConnectToChat()
        {
            try
            {
                ClientManager.MatchClient.JoinMatchChat(this.MatchCode, SessionManager.CurrentUsername);
            }
            catch (EndpointNotFoundException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTimeoutChat,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextInvalid,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void CheckForBufferedCards()
        {
            if (TrucoCallbackHandler.BufferedHand != null)
            {
                ReceiveCards(TrucoCallbackHandler.BufferedHand);
                TrucoCallbackHandler.BufferedHand = null;
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

        protected virtual void UpdatePlayerHandUI(List<TrucoCard> hand)
        {
            try
            {
                if (PlayerCardImages == null)
                {
                    return;
                }

                for (int i = 0; i < PlayerCardImages.Length; i++)
                {
                    if (i < hand.Count)
                    {
                        PlayerCardImages[i].Source = LoadCardImage(hand[i].FileName);
                        PlayerCardImages[i].Tag = hand[i];
                        PlayerCardImages[i].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        PlayerCardImages[i].Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCardImages,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentNullException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextArgument,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PlayerCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Image clickedCard && clickedCard.Tag is TrucoCard card)
                {
                    clickedCard.Visibility = Visibility.Collapsed;
                    SendPlayCardCommand(card.FileName);
                }
            }
            catch (InvalidCastException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void SendPlayCardCommand(string cardFileName)
        {
            try 
            {
                MatchClient.PlayCard(MatchCode, cardFileName);
            }
            catch (CommunicationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTimeoutChat,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ObjectDisposedException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void SendCallTrucoCommand(string betType)
        {
            try 
            { 
                MatchClient.CallTruco(MatchCode, betType);
            }
            catch (CommunicationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTimeoutChat,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ObjectDisposedException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void SendResponseCommand(string response)
        {
            try
            {
                MatchClient.RespondToCall(MatchCode, response);
            }
            catch (CommunicationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTimeoutChat,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ObjectDisposedException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void SendCallEnvidoCommand(string betType)
        {
            try
            {
                MatchClient.CallEnvido(MatchCode, betType);
            }
            catch (CommunicationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTimeoutChat,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ObjectDisposedException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void SendRespondToEnvidoCommand(string response)
        {
            try
            {
                MatchClient.RespondToEnvido(MatchCode, response);
            }
            catch (CommunicationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTimeoutChat,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ObjectDisposedException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void SendRespondToFlorCommand(string response)
        {
            try
            {
                MatchClient.RespondToFlor(MatchCode, response);
            }
            catch (CommunicationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTimeoutChat,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ObjectDisposedException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void SendGoToDeckCommand()
        {
            try
            {
                MatchClient.GoToDeck(MatchCode);
            }
            catch (CommunicationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTimeoutChat,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ObjectDisposedException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
            catch (CommunicationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTimeoutChat,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ObjectDisposedException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReceiveCards(List<TrucoCard> hand)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    this.currentTrucoBetState = BET_NONE;
                    this.playerHand = hand;
                    this.florPlayedInCurrentHand = false;
                    UpdatePlayerHandUI(hand);
                });
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTaskCanceled,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NotifyCardPlayed(string playerName, string cardFileName, bool isLastCardOfRound)
        {
            try
            {
                Dispatcher.Invoke(() => UpdatePlayedCardUI(playerName, cardFileName, isLastCardOfRound));
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTaskCanceled,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NotifyTurnChange(string nextPlayerName)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    HideFlorBetPanelUI();
                    UpdateTurnUI(nextPlayerName, this.currentTrucoBetState);
                });
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTaskCanceled,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NotifyScoreUpdate(int team1Score, int team2Score)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    TxtScoreTeam1.Text = team1Score.ToString();
                    TxtScoreTeam2.Text = team2Score.ToString();
                });
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTaskCanceled,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NotifyTrucoCall(string callerName, string betName, bool needsResponse)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateBetPanelUI(callerName, betName, needsResponse);
                    AddChatMessage(null, string.Format(Lang.GameTextPlayerCalledBet, callerName, betName));
                });
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTaskCanceled,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NotifyEnvidoCall(string callerName, string betName, bool needsResponse)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    HideBetPanelUI();
                    UpdateEnvidoBetPanelUI(callerName, betName, needsResponse);
                    AddChatMessage(null, string.Format(Lang.GameTextPlayerCalledBet, callerName, betName));
                });
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTaskCanceled,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NotifyFlorCall(string callerName, string betName, int totalPoints, bool needsResponse)
        {
            try
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
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTaskCanceled,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NotifyEnvidoFlorResult(string winnerName, int score)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    HideEnvidoBetPanelUI();
                    HideFlorBetPanelUI();
                    AddChatMessage(null, string.Format(Lang.GameTextBetWonBy, winnerName, score));
                });
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTaskCanceled,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NotifyResponse(string responderName, string response, string newBetState)
        {
            try
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
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTaskCanceled,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NotifyRoundEnd(string winnerName, int team1Score, int team2Score)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    AddChatMessage(null, string.Format(Lang.GameTextRoundWonBy, winnerName));
                    TxtScoreTeam1.Text = team1Score.ToString();
                    TxtScoreTeam2.Text = team2Score.ToString();
                    ClearTableUI();
                });
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTaskCanceled,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        protected void UpdateTurnButtons(bool isMyTurn, string currentBetState)
        {
            try
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
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void UpdateBetPanelUI(string callerName, string currentBet, bool needsResponse)
        {
            try
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
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void UpdateEnvidoBetPanelUI(string callerName, string currentBet, bool needsResponse)
        {
            try
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
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void UpdateFlorBetPanelUI(string callerName, string currentBet, bool needsResponse)
        {
            try
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
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void HideBetPanelUI()
        {
            try
            {
                PanelBetOptions.Visibility = Visibility.Collapsed;
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void HideEnvidoBetPanelUI()
        {
            try
            { 
                PanelEnvidoOptions.Visibility = Visibility.Collapsed;
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void HideFlorBetPanelUI()
        {
            try
            {
                PanelFlorOptions.Visibility = Visibility.Collapsed;
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void ClearTableUI()
        {
            try
            {
                PanelTableCards.Children.Clear();
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextThreadsDispatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
                catch (Exception) 
                {
                    CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, Lang.GlobalTextRuntimeError,
                        MessageBoxButton.OK, MessageBoxImage.Error);
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
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
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
                    ClientManager.MatchClient.SendChatMessage(this.MatchCode, currentPlayer, emoji); 
                }
                catch (Exception) 
                {
                    CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
            catch (UriFormatException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextAvatarIdFailedToLoad,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);

                return new BitmapImage(new Uri(DEFAULT_AVATAR_PATH, UriKind.Relative));
            }
            catch (ArgumentNullException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorLoadingAvatar,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);

                return new BitmapImage(new Uri(DEFAULT_AVATAR_PATH, UriKind.Relative));
            }
            catch (FileNotFoundException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextFileNotFound,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);

                return new BitmapImage(new Uri(DEFAULT_AVATAR_PATH, UriKind.Relative));
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);

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
            catch (UriFormatException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextAvatarIdFailedToLoad,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);

                return new BitmapImage(new Uri(DEFAULT_AVATAR_PATH, UriKind.Relative));
            }
            catch (ArgumentNullException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorLoadingAvatar,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);

                return new BitmapImage(new Uri(DEFAULT_AVATAR_PATH, UriKind.Relative));
            }
            catch (FileNotFoundException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextFileNotFound,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);

                return new BitmapImage(new Uri(DEFAULT_AVATAR_PATH, UriKind.Relative));
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);

                return new BitmapImage(new Uri(DEFAULT_AVATAR_PATH, UriKind.Relative));
            }
        }
    }
}