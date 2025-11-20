using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class GameTwoPlayersPage : GameBasePage
    {
        private readonly List<PlayerInfo> players;
        private Image[] cardImages;
        private List<TrucoCard> playerHand = new List<TrucoCard>();

        private const int CARDS_IN_HAND = 3;
        private const int WIDTH_CARD = 100;
        private const int HEIGHT_CARD = 150;
        private const double OPACITY_ACTIVE = 1.0;
        private const double OPACITY_INACTIVE = 0.5;

        private const string BET_STATUS_NONE = "None";
        private const string BET_TRUCO = "Truco";
        private const string BET_RETRUCO = "Retruco";
        private const string BET_VALE_CUATRO = "ValeCuatro";

        private const string RESPOND_QUIERO = "Quiero";
        private const string RESPOND_NO_QUIERO = "NoQuiero";

        private const string BET_ENVIDO = "Envido";
        private const string BET_REAL_ENVIDO = "RealEnvido";
        private const string BET_FALTA_ENVIDO = "FaltaEnvido";

        private const string BET_FLOR = "Flor";
        private const string BET_CONTRA_FLOR = "ContraFlor";
        private const string BET_CONTRA_FLOR_AL_RESTO = "ContraFlorAlResto";

        protected override TextBlock TbScoreTeam1 => tbScoreTeam1;
        protected override TextBlock TbScoreTeam2 => tbScoreTeam2;
        protected override StackPanel PanelPlayerCards => panelPlayerCards;
        protected override StackPanel PanelTableCards => panelTableCards;
        protected override StackPanel PanelBetOptions => panelBetOptions;
        protected override StackPanel PanelEnvidoOptions => panelEnvidoOptions;

        public GameTwoPlayersPage(string matchCode, List<PlayerInfo> players)
        {
            InitializeComponent();
            
            base.InitializeBase(matchCode, this.txtChatMessage, this.ChatMessagesPanel, this.blckPlaceholder);

            this.players = players;
            this.Loaded += GamePage_Loaded;

            cardImages = new[]
            {
                imgPlayerCard1,
                imgPlayerCard2,
                imgPlayerCard3
            };

            foreach (var img in cardImages)
            {
                img.MouseDown += PlayerCard_MouseDown;
            }

            btnCallTruco.Click += (s, e) => 
            {
                string betToSend = (s as Button).Content.ToString();
                SendCallTrucoCommand(betToSend);
            };

            btnRespondQuiero.Click += (s, e) => SendResponseCommand(RESPOND_QUIERO);
            btnRespondNoQuiero.Click += (s, e) => SendResponseCommand(RESPOND_NO_QUIERO); 
            PanelPlayerCards.IsEnabled = false;

            btnCallEnvido.Click += (s, e) => SendCallEnvidoCommand(BET_ENVIDO);
            btnCallRealEnvido.Click += (s, e) => SendCallEnvidoCommand(BET_REAL_ENVIDO);
            btnCallFaltaEnvido.Click += (s, e) => SendCallEnvidoCommand(BET_FALTA_ENVIDO);
            btnEnvidoRespondQuiero.Click += (s, e) => SendRespondToEnvidoCommand(RESPOND_QUIERO);
            btnEnvidoRespondNoQuiero.Click += (s, e) => SendRespondToEnvidoCommand(RESPOND_NO_QUIERO);

            btnGoToDeck.Click += (s, e) => SendGoToDeckCommand();

            btnStartFlor.Click += (s, e) => SendCallFlorCommand(BET_FLOR);
            btnCallFlor.Click += (s, e) => SendCallFlorCommand(BET_FLOR); 
            btnCallContraFlor.Click += (s, e) => SendCallFlorCommand(BET_CONTRA_FLOR);
            btnCallContraFlorAlResto.Click += (s, e) => SendCallFlorCommand(BET_CONTRA_FLOR_AL_RESTO);

            btnFlorRespondQuiero.Click += (s, e) => SendRespondToFlorCommand(RESPOND_QUIERO);
            btnFlorRespondNoQuiero.Click += (s, e) => SendRespondToFlorCommand(RESPOND_NO_QUIERO);
        }

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPlayerAvatars(players);
            CheckForBufferedCards();
            this.Loaded -= GamePage_Loaded;
        }

        protected override void LoadPlayerAvatars(List<PlayerInfo> players)
        {
            try
            {
                var current = players.FirstOrDefault(p => p.Username.Equals(CurrentPlayer, StringComparison.OrdinalIgnoreCase));
                var rival = players.FirstOrDefault(p => !p.Username.Equals(CurrentPlayer, StringComparison.OrdinalIgnoreCase));

                if (current != null)
                {
                    imgPlayerAvatar.Source = LoadAvatar(current.AvatarId);
                }

                imgRivalAvatar.Source = LoadAvatar(rival != null ? rival.AvatarId : DEFAULT_AVATAR_ID);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextErrorLoadingAvatar, ex.Message), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void UpdatePlayerHandUI(List<TrucoCard> hand)
        {
            this.playerHand = hand;
            for (int i = 0; i < CARDS_IN_HAND; i++)
            {

                if (i < hand.Count)
                {
                    cardImages[i].Source = LoadCardImage(hand[i].FileName);
                    cardImages[i].Tag = hand[i]; 
                    cardImages[i].Visibility = Visibility.Visible;
                }
                else
                {
                    cardImages[i].Visibility = Visibility.Hidden;
                }
            }
        }

        protected override void UpdatePlayedCardUI(string playerName, string cardFileName, bool isLastCardOfRound)
        {
            Image cardImage = new Image
            {
                Source = LoadCardImage(cardFileName),
                Width = WIDTH_CARD,
                Height = HEIGHT_CARD,
                Margin = new Thickness(10)
            };

            if (playerName == CurrentPlayer)
            {
                cardImage.VerticalAlignment = VerticalAlignment.Top;
            }
            else
            {
                cardImage.VerticalAlignment = VerticalAlignment.Top;
            }
            PanelTableCards.Children.Add(cardImage);
        }

        protected override void UpdateTurnUI(string nextPlayerName, string currentBetState)
        {
            bool isMyTurn = nextPlayerName == CurrentPlayer;
            PanelPlayerCards.IsEnabled = isMyTurn;

            imgPlayerAvatar.Opacity = isMyTurn ? OPACITY_ACTIVE : OPACITY_INACTIVE;
            imgRivalAvatar.Opacity = isMyTurn ? OPACITY_INACTIVE : OPACITY_ACTIVE;

            if (isMyTurn)
            {
                PanelBetOptions.Visibility = Visibility.Visible;
                btnRespondQuiero.Visibility = Visibility.Collapsed;
                btnRespondNoQuiero.Visibility = Visibility.Collapsed;

                btnCallTruco.Visibility = Visibility.Visible;
                btnGoToDeck.Visibility = Visibility.Visible;

                if (currentBetState == BET_STATUS_NONE && ClientHasFlor())
                {
                    btnStartFlor.Visibility = Visibility.Visible;
                }
                else
                {
                    btnStartFlor.Visibility = Visibility.Collapsed;
                }

                if (currentBetState == BET_STATUS_NONE)
                {
                    btnCallTruco.Content = BET_TRUCO;
                }
                else if (currentBetState == BET_TRUCO)
                {
                    btnCallTruco.Content = BET_RETRUCO;
                }
                else if (currentBetState == BET_RETRUCO)
                {
                    btnCallTruco.Content = BET_VALE_CUATRO;
                }
                else
                {
                    btnCallTruco.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                PanelBetOptions.Visibility = Visibility.Collapsed;
                btnGoToDeck.Visibility = Visibility.Collapsed;
            }
        }

        protected override void UpdateBetPanelUI(string callerName, string currentBet, bool needsResponse)
        {
            if (needsResponse)
            {
                PanelBetOptions.Visibility = Visibility.Visible;
                btnCallTruco.Visibility = Visibility.Collapsed;
                btnStartFlor.Visibility = Visibility.Collapsed;
                btnRespondQuiero.Visibility = Visibility.Visible;
                btnRespondNoQuiero.Visibility = Visibility.Visible;
            }
            else
            {
                PanelBetOptions.Visibility = Visibility.Collapsed;
            }
        }

        protected override void HideBetPanelUI()
        {
            PanelBetOptions.Visibility = Visibility.Collapsed;
        }

        protected override void ClearTableUI()
        {
            PanelTableCards.Children.Clear();
        }

        private void PlayerCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image clickedCard && clickedCard.Tag is TrucoCard card)
            {
                clickedCard.Visibility = Visibility.Collapsed;
                SendPlayCardCommand(card.FileName);
            }
        }

        protected override void UpdateEnvidoBetPanelUI(string callerName, string currentBet, int totalPoints, bool needsResponse)
        {
            PanelEnvidoOptions.Visibility = Visibility.Visible;
            tbEnvidoCaller.Text = $"{callerName} cantó {currentBet} ({totalPoints} puntos)";
            bool isMyTurnToRespond = needsResponse && (callerName != CurrentPlayer);
            btnEnvidoRespondQuiero.Visibility = isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
            btnEnvidoRespondNoQuiero.Visibility = isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;

            // TODO: Lógica más avanzada para mostrar solo las apuestas válidas (ej. no mostrar "Envido" si ya se cantó "Envido")
            btnCallEnvido.Visibility = !isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
            btnCallRealEnvido.Visibility = !isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
            btnCallFaltaEnvido.Visibility = !isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void HideEnvidoBetPanelUI()
        {
            PanelEnvidoOptions.Visibility = Visibility.Collapsed;
        }

        protected override void NotifyEnvidoResultUI(string winnerName, int score, int totalPointsAwarded)
        {
            HideEnvidoBetPanelUI();
        }

        private void ClickCallTruco(object sender, RoutedEventArgs e)
        {
            string betToSend = (sender as Button).Content.ToString();
            SendCallTrucoCommand(betToSend);
        }

        private void ClickRespondQuiero(object sender, RoutedEventArgs e)
        {

        }

        private void ClickRespondNoQuiero(object sender, RoutedEventArgs e)
        {

        }

        private void ClickAlMazo(object sender, RoutedEventArgs e)
        {

        }

        protected override void UpdateFlorBetPanelUI(string callerName, string currentBet, int totalPoints, bool needsResponse)
        {
            panelFlorOptions.Visibility = Visibility.Visible;
            tbFlorCaller.Text = $"{callerName} cantó {currentBet} ({totalPoints} puntos)";
            bool isMyTurnToRespond = needsResponse && (callerName != CurrentPlayer);
            btnFlorRespondQuiero.Visibility = isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
            btnFlorRespondNoQuiero.Visibility = isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
            btnCallFlor.Visibility = Visibility.Collapsed; 
            btnCallContraFlor.Visibility = isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
            btnCallContraFlorAlResto.Visibility = isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;

            if (!isMyTurnToRespond)
            {
                tbFlorCaller.Text = $"{callerName} cantó {currentBet}. Esperando respuesta...";
            }
            else
            {
                tbFlorCaller.Text = $"{callerName} cantó {currentBet} ({totalPoints} puntos)";
            }
        }

        protected override void HideFlorBetPanelUI()
        {
            panelFlorOptions.Visibility = Visibility.Collapsed;
        }

        private bool ClientHasFlor()
        {
            if (playerHand == null || playerHand.Count < 3)
            {
                return false;
            }

            var groups = playerHand.GroupBy(card => card.CardSuit);
            
            return groups.Any(g => g.Count() >= 3);
        }
    }
}