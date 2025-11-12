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
            base.InitializeBase(matchCode, this.txtChatMessage, this.ChatMessagesPanel, this.blckPlaceholder);

            this.players = players;

            this.Loaded += GamePage_Loaded;
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

    }
}