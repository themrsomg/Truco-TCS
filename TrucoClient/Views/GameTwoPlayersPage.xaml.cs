using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
    }
}