using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class GameTwoPlayersPage : GameBasePage
    {
        private readonly List<PlayerInfo> players;
        private Image[] cardImages;
        private const int CARDS_IN_HAND = 3;

        protected override TextBlock TbScoreTeam1 => tbScoreTeam1;
        protected override TextBlock TbScoreTeam2 => tbScoreTeam2;
        protected override StackPanel PanelPlayerCards => panelPlayerCards;
        protected override StackPanel PanelTableCards => panelTableCards;
        protected override StackPanel PanelBetOptions => panelBetOptions;
        protected override StackPanel PanelEnvidoOptions => panelEnvidoOptions;

        public GameTwoPlayersPage(string matchCode, List<PlayerInfo> players)
        {
            InitializeComponent();
            cardImages = new[] 
            { 
                imgPlayerCard1, 
                imgPlayerCard2, 
                imgPlayerCard3 
            };
            base.InitializeBase(matchCode, this.txtChatMessage, this.ChatMessagesPanel, this.blckPlaceholder);
            this.players = players;
            this.Loaded += GamePage_Loaded;
            foreach (var img in cardImages)
            {
                img.MouseDown += PlayerCard_MouseDown;
            }

            btnCallTruco.Click += (s, e) => {
                string betToSend = (s as Button).Content.ToString();
                SendCallTrucoCommand(betToSend);
            };
            btnRespondQuiero.Click += (s, e) => SendResponseCommand("Quiero");
            btnRespondNoQuiero.Click += (s, e) => SendResponseCommand("NoQuiero"); 
            PanelPlayerCards.IsEnabled = false;

            btnCallEnvido.Click += (s, e) => SendCallEnvidoCommand("Envido");
            btnCallRealEnvido.Click += (s, e) => SendCallEnvidoCommand("RealEnvido");
            btnCallFaltaEnvido.Click += (s, e) => SendCallEnvidoCommand("FaltaEnvido");
            btnEnvidoRespondQuiero.Click += (s, e) => SendRespondToEnvidoCommand("Quiero");
            btnEnvidoRespondNoQuiero.Click += (s, e) => SendRespondToEnvidoCommand("NoQuiero");
        }

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPlayerAvatars(players);
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
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorLoadingAvatar, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void UpdatePlayerHandUI(List<TrucoCard> hand)
        {
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
                Source = new BitmapImage(new Uri($"/Resources/Cards/{cardFileName}.png", UriKind.Relative)),
                Width = 100,
                Height = 150,
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

        // TODO: Realizar correcciones referentes al llamado de apuestas
        protected override void UpdateTurnUI(string nextPlayerName, string currentBetState)
        {
            bool isMyTurn = nextPlayerName == CurrentPlayer;
            PanelPlayerCards.IsEnabled = isMyTurn;

            imgPlayerAvatar.Opacity = isMyTurn ? 1.0 : 0.5;
            imgRivalAvatar.Opacity = isMyTurn ? 0.5 : 1.0;

            if (isMyTurn)
            {
                PanelBetOptions.Visibility = Visibility.Visible;
                btnRespondQuiero.Visibility = Visibility.Collapsed;
                btnRespondNoQuiero.Visibility = Visibility.Collapsed;
                btnCallTruco.Visibility = Visibility.Visible;
                if (currentBetState == "None")
                {
                    btnCallTruco.Content = "Truco";
                }
                else if (currentBetState == "Truco")
                {
                    btnCallTruco.Content = "Retruco";
                }
                else if (currentBetState == "Retruco")
                {
                    btnCallTruco.Content = "ValeCuatro";
                }
                else
                {
                    btnCallTruco.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                PanelBetOptions.Visibility = Visibility.Collapsed;
            }
        }

        protected override void UpdateBetPanelUI(string callerName, string currentBet, bool needsResponse)
        {
            if (needsResponse)
            {
                PanelBetOptions.Visibility = Visibility.Visible;
                btnCallTruco.Visibility = Visibility.Collapsed;
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
    }
}