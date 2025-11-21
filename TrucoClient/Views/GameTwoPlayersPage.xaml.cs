using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class GameTwoPlayersPage : GameBasePage
    {
        private readonly List<PlayerInfo> players;
        private Image[] cardImages;
        private const double OPACITY_ACTIVE = 1.0;
        private const double OPACITY_INACTIVE = 0.5;

        protected override TextBlock TxtScoreTeam1 => blckScoreTeam1;
        protected override TextBlock TxtScoreTeam2 => blckScoreTeam2;
        protected override StackPanel PanelPlayerCards => panelPlayerCards;
        protected override StackPanel PanelTableCards => panelTableCards;
        protected override StackPanel PanelBetOptions => panelBetOptions;
        protected override StackPanel PanelEnvidoOptions => panelEnvidoOptions;
        protected override StackPanel PanelFlorOptions => panelFlorOptions;

        protected override TextBlock TxtEnvidoCaller => blckEnvidoCaller;
        protected override TextBlock TxtFlorCaller => blckFlorCaller;
        protected override TextBlock TxtTrucoCaller => blckTrucoCaller;

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

        public GameTwoPlayersPage(string matchCode, List<PlayerInfo> players)
        {
            InitializeComponent();

            base.InitializeBase(matchCode, this.txtChatMessage, this.ChatMessagesPanel, this.blckPlaceholder);

            this.players = players;
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
                var current = players.FirstOrDefault(p => p.Username.Equals(currentPlayer, StringComparison.OrdinalIgnoreCase));
                var rival = players.FirstOrDefault(p => !p.Username.Equals(currentPlayer, StringComparison.OrdinalIgnoreCase));

                if (current != null) imgPlayerAvatar.Source = LoadAvatar(current.AvatarId);
                imgRivalAvatar.Source = LoadAvatar(rival != null ? rival.AvatarId : DEFAULT_AVATAR_ID);
            }
            catch { /* error */ }
        }

        protected override void UpdateTurnUI(string nextPlayerName, string currentBetState)
        {
            bool isMyTurn = nextPlayerName == currentPlayer;
            PanelPlayerCards.IsEnabled = isMyTurn;

            imgPlayerAvatar.Opacity = isMyTurn ? OPACITY_ACTIVE : OPACITY_INACTIVE;
            imgRivalAvatar.Opacity = isMyTurn ? OPACITY_INACTIVE : OPACITY_ACTIVE;

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