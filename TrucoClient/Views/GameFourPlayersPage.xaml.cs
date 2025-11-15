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
    public partial class GameFourPlayersPage : GameBasePage
    {
        private readonly List<PlayerInfo> players;
        private List<TrucoCard> playerHand = new List<TrucoCard>();
        private Image[] cardImages;

        protected override TextBlock TbScoreTeam1 => tbScoreTeam1;
        protected override TextBlock TbScoreTeam2 => tbScoreTeam2;
        protected override StackPanel PanelPlayerCards => panelPlayerCards;
        protected override StackPanel PanelTableCards => panelTableCards;
        protected override StackPanel PanelBetOptions => panelBetOptions;
        protected override StackPanel PanelEnvidoOptions => throw new NotImplementedException();

        public GameFourPlayersPage(string matchCode, List<PlayerInfo> players)
        {
            InitializeComponent();
            cardImages = new[] { imgPlayerCard1, imgPlayerCard2, imgPlayerCard3 };
            base.InitializeBase(matchCode, this.txtChatMessage, this.ChatMessagesPanel, this.blckPlaceholder);
            this.players = players ?? new List<PlayerInfo>();
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
                if (players == null || players.Count == 0)
                {
                    return;
                }

                var self = players.FirstOrDefault(p => p.Username.Equals(CurrentPlayer, StringComparison.OrdinalIgnoreCase));
                if (self == null)
                {
                    MessageBox.Show(Lang.GameTextPlayerNotFound, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                imgPlayerAvatar.Source = LoadAvatar(self.AvatarId);

                var allies = players
                    .Where(p => p.Team == self.Team && !p.Username.Equals(CurrentPlayer, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var enemies = players
                    .Where(p => p.Team != self.Team)
                    .ToList();

                if (allies.Count > 0)
                {
                    imgTopAvatar.Source = LoadAvatar(allies[0].AvatarId);
                }

                if (enemies.Count > 0)
                {
                    imgLeftAvatar.Source = LoadAvatar(enemies[0].AvatarId);
                }

                if (enemies.Count > 1)
                {
                    imgRightAvatar.Source = LoadAvatar(enemies[1].AvatarId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorLoadingAvatar, ex.Message),
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void UpdatePlayerHandUI(List<TrucoCard> hand)
        {
            for (int i = 0; i < cardImages.Length; i++)
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

            // TODO: Determinar la posición (Top, Left, Right, Bottom) basada en el playerName y el layout de 4 jugadores.

            PanelTableCards.Children.Add(cardImage);
        }

        protected override void UpdateTurnUI(string nextPlayerName, string currentBetState)
        {
            bool isMyTurn = nextPlayerName == CurrentPlayer;
            PanelPlayerCards.IsEnabled = isMyTurn;
            imgPlayerAvatar.Opacity = 0.5;
            imgLeftAvatar.Opacity = 0.5;
            imgRightAvatar.Opacity = 0.5;
            if (isMyTurn)
            {
                imgPlayerAvatar.Opacity = 1.0;
            }
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

        private void ClickTruco(object sender, RoutedEventArgs e)
        {

        }

        private void ClickAccept(object sender, RoutedEventArgs e)
        {

        }

        private void ClickReject(object sender, RoutedEventArgs e)
        {

        }

        protected override void UpdateEnvidoBetPanelUI(string callerName, string currentBet, int totalPoints, bool needsResponse)
        {
            PanelEnvidoOptions.Visibility = Visibility.Visible;
            tbEnvidoCaller.Text = $"{callerName} cantó {currentBet} ({totalPoints} puntos)";

            bool isMyTurnToRespond = needsResponse && (callerName != CurrentPlayer);
            btnEnvidoRespondQuiero.Visibility = isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
            btnEnvidoRespondNoQuiero.Visibility = isMyTurnToRespond ? Visibility.Visible : Visibility.Collapsed;
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
    }
}