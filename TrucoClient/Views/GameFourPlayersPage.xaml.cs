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
        private string topPlayerName;
        private string leftPlayerName;
        private string rightPlayerName;

        public GameFourPlayersPage(string matchCode, List<PlayerInfo> players)
        {
            InitializeComponent();
            MapUiControls();
            base.InitializeBase(matchCode, this.txtChatMessage, this.ChatMessagesPanel, this.blckPlaceholder);

            this.players = players ?? new List<PlayerInfo>();
            base.CurrentMatchPlayers = this.players;

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

                int myIndex = players.IndexOf(self);

                Console.WriteLine($"[LOAD AVATARS] My position: {myIndex}, My username: {currentPlayer}, My team: {self.Team}");
                Console.WriteLine($"[LOAD AVATARS] Full player list:");
                for (int i = 0; i < players.Count; i++)
                {
                    Console.WriteLine($"  [{i}] {players[i].Username} - {players[i].Team}");
                }

                int topIndex = (myIndex + 2) % 4;
                int leftIndex = (myIndex + 1) % 4;
                int rightIndex = (myIndex + 3) % 4;

                imgTopAvatar.Source = LoadAvatar(players[topIndex].AvatarId);
                topPlayerName = players[topIndex].Username;

                imgLeftAvatar.Source = LoadAvatar(players[leftIndex].AvatarId);
                leftPlayerName = players[leftIndex].Username;

                imgRightAvatar.Source = LoadAvatar(players[rightIndex].AvatarId);
                rightPlayerName = players[rightIndex].Username;

                Console.WriteLine($"[LOAD AVATARS] Visual layout:");
                Console.WriteLine($"  Me (Bottom): {currentPlayer} (pos {myIndex}, {self.Team})");
                Console.WriteLine($"  Top: {topPlayerName} (pos {topIndex}, {players[topIndex].Team})");
                Console.WriteLine($"  Left: {leftPlayerName} (pos {leftIndex}, {players[leftIndex].Team})");
                Console.WriteLine($"  Right: {rightPlayerName} (pos {rightIndex}, {players[rightIndex].Team})");

                if (players[topIndex].Team != self.Team)
                {
                    Console.WriteLine($"[LOAD AVATARS ERROR] Top player {topPlayerName} should be teammate but is {players[topIndex].Team}!");
                }
                else
                {
                    Console.WriteLine($"[LOAD AVATARS] ✓ Top player is correctly my teammate");
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

            if (nextPlayerName == topPlayerName && imgTopAvatar != null)
            { 
                imgTopAvatar.Opacity = OPACITY_ACTIVE;
            }
            else if (nextPlayerName == leftPlayerName)
            {
                imgLeftAvatar.Opacity = OPACITY_ACTIVE;
            }
            else if (nextPlayerName == rightPlayerName)
            {
                imgRightAvatar.Opacity = OPACITY_ACTIVE;
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

            base.txtBaseChatMessage = txtChatMessage;
            base.blckBasePlaceholder = blckPlaceholder;
        }
    }
}