using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class GameFourPlayersPage : GameBasePage
    {
        private readonly List<PlayerInfo> players;

        public GameFourPlayersPage(string matchCode, List<PlayerInfo> players)
        {
            InitializeComponent();

            MapUiControls();

            base.InitializeBase(matchCode, this.txtChatMessage, this.ChatMessagesPanel, this.blckPlaceholder);

            this.players = players ?? new List<PlayerInfo>();
            this.Loaded += GamePage_Loaded;

            base.PlayerCardImages = new[] { imgPlayerCard1, imgPlayerCard2, imgPlayerCard3 };

            InitializeGameEvents();
            InitializeCardEvents();

            PanelPlayerCards.IsEnabled = false;
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
            catch (ArgumentNullException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextArgument,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void UpdateTurnUI(string nextPlayerName, string currentBetState)
        {
            bool isMyTurn = nextPlayerName == currentPlayer;
            PanelPlayerCards.IsEnabled = isMyTurn;

            imgPlayerAvatar.Opacity = isMyTurn ? OPACITY_ACTIVE : OPACITY_INACTIVE;

            imgLeftAvatar.Opacity = OPACITY_INACTIVE;
            imgRightAvatar.Opacity = OPACITY_INACTIVE;

            if (imgTopAvatar != null)
            {
                imgTopAvatar.Opacity = OPACITY_INACTIVE;
            }

            base.UpdateTurnButtons(isMyTurn, currentBetState);
        }

        private void MapUiControls()
        {
            base.BtnBack = btnBack;

            base.TxtScoreTeam1 = blckScoreTeam1;
            base.TxtScoreTeam2 = blckScoreTeam2;
            base.TxtEnvidoCaller = blckEnvidoCaller;
            base.TxtFlorCaller = blckFlorCaller;
            base.TxtTrucoCaller = blckTrucoCaller;

            base.PanelPlayerCards = panelPlayerCards;
            base.PanelTableCards = panelTableCards;
            base.PanelBetOptions = panelBetOptions;
            base.PanelEnvidoOptions = panelEnvidoOptions;
            base.PanelFlorOptions = panelFlorOptions;

            base.BtnCallTruco = btnCallTruco;
            base.BtnRespondQuiero = btnRespondQuiero;
            base.BtnRespondNoQuiero = btnRespondNoQuiero;
            base.BtnGoToDeck = btnGoToDeck;

            base.BtnCallEnvido = btnCallEnvido;
            base.BtnCallRealEnvido = btnCallRealEnvido;
            base.BtnCallFaltaEnvido = btnCallFaltaEnvido;
            base.BtnEnvidoRespondQuiero = btnEnvidoRespondQuiero;
            base.BtnEnvidoRespondNoQuiero = btnEnvidoRespondNoQuiero;

            base.BtnStartFlor = btnStartFlor;
            base.BtnCallFlor = btnCallFlor;
            base.BtnCallContraFlor = btnCallContraFlor;
        }
    }
}