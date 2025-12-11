using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class GameTwoPlayersPage : GameBasePage
    {
        private readonly List<PlayerInfo> players;

        public GameTwoPlayersPage(string matchCode, List<PlayerInfo> players)
        {
            InitializeComponent();

            MapUiControls();

            base.InitializeBase(matchCode, this.txtChatMessage, this.ChatMessagesPanel, this.blckPlaceholder);

            this.players = players;
            this.Loaded += GamePage_Loaded;

            base.PlayerCardImages = new[] 
            { 
                imgPlayerCard1, 
                imgPlayerCard2, 
                imgPlayerCard3 
            };

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
            if (players == null || players.Count == 0)
            {
                return;
            }

            try
            {
                var current = players.FirstOrDefault(p => p.Username.Equals(currentPlayer, StringComparison.OrdinalIgnoreCase));
                var rival = players.FirstOrDefault(p => !p.Username.Equals(currentPlayer, StringComparison.OrdinalIgnoreCase));

                if (current != null)
                {
                    imgPlayerAvatar.Source = LoadAvatar(current.AvatarId);
                }

                imgRivalAvatar.Source = LoadAvatar(rival != null ? rival.AvatarId : DEFAULT_AVATAR_ID);
            }
            catch (ArgumentNullException ex)
            {
                ClientException.HandleError(ex, nameof(LoadPlayerAvatars));
                CustomMessageBox.Show(Lang.ExceptionTextArgument,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException ex)
            {
                ClientException.HandleError(ex, nameof(LoadPlayerAvatars));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(LoadPlayerAvatars));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void UpdateTurnUI(string nextPlayerName, string currentBetState)
        {
            bool isMyTurn = nextPlayerName == currentPlayer;
            PanelPlayerCards.IsEnabled = isMyTurn;

            imgPlayerAvatar.Opacity = isMyTurn ? OPACITY_ACTIVE : OPACITY_INACTIVE;
            imgRivalAvatar.Opacity = isMyTurn ? OPACITY_INACTIVE : OPACITY_ACTIVE;

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

            base.txtBaseChatMessage = txtChatMessage;
            base.blckBasePlaceholder = blckPlaceholder;

        }
    }
}