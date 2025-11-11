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

                int selfIndex = players.FindIndex(p => p.Username.Equals(CurrentPlayer, StringComparison.OrdinalIgnoreCase));
                if (selfIndex != -1)
                {
                    imgPlayerAvatar.Source = LoadAvatar(players[selfIndex].AvatarId);

                    if (players.Count >= 4)
                    {
                        imgLeftAvatar.Source = LoadAvatar(players[(selfIndex + 1) % 4].AvatarId);
                        imgTopAvatar.Source = LoadAvatar(players[(selfIndex + 2) % 4].AvatarId);
                        imgRightAvatar.Source = LoadAvatar(players[(selfIndex + 3) % 4].AvatarId);
                    }
                    else
                    {
                        var rivals = players.Where(p => !p.Username.Equals(CurrentPlayer, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (rivals.Count > 0)
                        {
                            imgTopAvatar.Source = LoadAvatar(rivals[0].AvatarId);
                        }
                        if (rivals.Count > 1)
                        {
                            imgLeftAvatar.Source = LoadAvatar(rivals[1].AvatarId);
                        }
                        if (rivals.Count > 2)
                        {
                            imgRightAvatar.Source = LoadAvatar(rivals[2].AvatarId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorLoadingAvatar, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}