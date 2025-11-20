using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class GameFourPlayersPage : GameBasePage
    {
        private readonly List<PlayerInfo> players;
        private Image[] cardImages;
        private const int CARDS_IN_HAND = 3;
        private const double OPACITY_DIMMED = 0.5;
        private const double OPACITY_FULL = 1.0;

        protected override TextBlock TxtScoreTeam1 => tbScoreTeam1;
        protected override TextBlock TxtScoreTeam2 => tbScoreTeam2;
        protected override StackPanel PanelPlayerCards => panelPlayerCards;
        protected override StackPanel PanelTableCards => panelTableCards;
        protected override StackPanel PanelBetOptions => panelBetOptions;
        protected override StackPanel PanelEnvidoOptions => panelEnvidoOptions;
        protected override StackPanel PanelFlorOptions => panelFlorOptions;

        protected override TextBlock TxtEnvidoCaller => tbEnvidoCaller;
        protected override TextBlock TxtFlorCaller => tbFlorCaller;

        protected override Button BtnCallTruco => btnCallTruco;
        protected override Button BtnRespondQuiero => btnRespondQuiero;
        protected override Button BtnRespondNoQuiero => btnRespondNoQuiero;
        protected override Button BtnGoToDeck => btnGoToDeck;

        protected override Button BtnCallEnvido => btnCallEnvido;
        protected override Button BtnCallRealEnvido => btnCallRealEnvido;
        protected override Button BtnCallFaltaEnvido => btnCallFaltaEnvido;
        protected override Button BtnEnvidoRespondQuiero => btnEnvidoRespondQuiero;
        protected override Button BtnEnvidoRespondNoQuiero => btnEnvidoRespondNoQuiero;

        protected override Button BtnStartFlor => btnStartFlor;
        protected override Button BtnCallFlor => btnCallFlor;
        protected override Button BtnCallContraFlor => btnCallContraFlor;


        public GameFourPlayersPage(string matchCode, List<PlayerInfo> players)
        {
            InitializeComponent();

            base.InitializeBase(matchCode, this.txtChatMessage, this.ChatMessagesPanel, this.blckPlaceholder);

            this.players = players ?? new List<PlayerInfo>();
            this.Loaded += GamePage_Loaded;

            cardImages = new[] { imgPlayerCard1, imgPlayerCard2, imgPlayerCard3 };

            foreach (var img in cardImages)
            {
                img.MouseDown += PlayerCard_MouseDown;
            }

            SetupCommonEventHandlers(btnBack, btnCallTruco, btnRespondQuiero, btnRespondNoQuiero);

            PanelPlayerCards.IsEnabled = false;

            btnCallEnvido.Click += (s, e) => SendCallEnvidoCommand(BET_ENVIDO);
            btnCallRealEnvido.Click += (s, e) => SendCallEnvidoCommand(BET_REAL_ENVIDO);
            btnCallFaltaEnvido.Click += (s, e) => SendCallEnvidoCommand(BET_FALTA_ENVIDO);
            btnEnvidoRespondQuiero.Click += (s, e) => SendRespondToEnvidoCommand(RESPOND_QUIERO);
            btnEnvidoRespondNoQuiero.Click += (s, e) => SendRespondToEnvidoCommand(RESPOND_NO_QUIERO);

            btnGoToDeck.Click += (s, e) => SendGoToDeckCommand();

            btnStartFlor.Click += (s, e) => SendCallFlorCommand(BET_FLOR);
            btnCallContraFlor.Click += (s, e) => SendRespondToFlorCommand(BET_CONTRA_FLOR);
        }

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPlayerAvatars(players);
            CheckForBufferedCards();
            this.Loaded -= GamePage_Loaded;
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

        protected override void LoadPlayerAvatars(List<PlayerInfo> players)
        {
            try
            {
                if (players == null || players.Count == 0)
                {
                    return;
                }

                var self = players.FirstOrDefault(p => p.Username.Equals(currentPlayer, StringComparison.OrdinalIgnoreCase));
                
                if (self == null)
                {
                    return;
                }

                imgPlayerAvatar.Source = LoadAvatar(self.AvatarId);

                var allies = players.Where(p => p.Team == self.Team && !p.Username.Equals(currentPlayer, StringComparison.OrdinalIgnoreCase)).ToList();
                var enemies = players.Where(p => p.Team != self.Team).ToList();

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
            catch 
            { 
                /* error */ 
            }
        }

        protected override void UpdateTurnUI(string nextPlayerName, string currentBetState)
        {
            bool isMyTurn = nextPlayerName == currentPlayer;
            PanelPlayerCards.IsEnabled = isMyTurn;

            imgPlayerAvatar.Opacity = isMyTurn ? OPACITY_FULL : OPACITY_DIMMED;
            imgLeftAvatar.Opacity = OPACITY_DIMMED;
            imgRightAvatar.Opacity = OPACITY_DIMMED;

            base.UpdateTurnButtons(isMyTurn, currentBetState);
        }

        private void PlayerCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image clickedCard && clickedCard.Tag is TrucoCard card)
            {
                clickedCard.Visibility = Visibility.Collapsed;
                SendPlayCardCommand(card.FileName);
            }
        }
    }
}