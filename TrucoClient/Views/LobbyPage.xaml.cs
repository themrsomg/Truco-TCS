using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrucoClient.TrucoServer;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class LobbyPage : Page
    {
        private const int FONT_SIZE = 13;

        private readonly string matchCode;
        private readonly string matchName;

        public class PlayerLobbyInfo
        {
            public string Username { get; set; }
            public BitmapImage AvatarUri { get; set; }
        }

        public LobbyPage(string matchCode, string matchName)
        {
            InitializeComponent();
            this.matchCode = matchCode;
            this.matchName = matchName;
            txtLobbyTitle.Text = $"Lobby - {matchName} ({matchCode})";

            LoadPlayers();
            InitializeChat();
        }

        private void LoadPlayers()
        {
            UserProfileData currentUserData = SessionManager.CurrentUserData;
            PlayersList.ItemsSource = new List<PlayerLobbyInfo>
            {
                new PlayerLobbyInfo { Username = SessionManager.CurrentUsername, AvatarUri = LoadAvatar(currentUserData.AvatarId) },
                new PlayerLobbyInfo { Username = "Jugador2", AvatarUri = LoadAvatar("avatar_aaa_default") }
            };

            if (SessionManager.CurrentUsername == "Jugador1") //////HOSTT
            {
                btnStart.Visibility = Visibility.Visible;
            }
        }

        private BitmapImage LoadAvatar(string avatarId)
        {
            string path = $"pack://application:,,,/TrucoClient;component/Resources/Avatars/{avatarId}.png";
            return new BitmapImage(new System.Uri(path, System.UriKind.Absolute));
        }

        private void InitializeChat()
        {
            AddChatMessage(" ", string.Format(Lang.LobbyTextJoinedRoom, matchCode));
        }

        private void txtChatMessageTextChanged(object sender, TextChangedEventArgs e)
        {
            if (blckPlaceholder != null)
            {
                blckPlaceholder.Visibility = string.IsNullOrEmpty(txtChatMessage.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
        
        private void AddChatMessage(string senderName, string message)
        {
            TextBlock messageText = new TextBlock();

            if (senderName.Equals(" "))
            {
                messageText = new TextBlock
                {
                    Text = $"{message}",
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = FONT_SIZE
                };

                messageText.Foreground = Brushes.DarkGray;
                messageText.FontStyle = FontStyles.Italic;
            }
            else
            {
                messageText = new TextBlock
                {
                    Text = $"{senderName}: {message}",
                    TextWrapping = TextWrapping.Wrap,   
                    FontSize = FONT_SIZE
                };
            }
            ChatMessagesPanel.Children.Add(messageText);
        }

        private void ClickSendMessage(object sender, RoutedEventArgs e)
        {
            string text = txtChatMessage.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            AddChatMessage(SessionManager.CurrentUsername, text);
            txtChatMessage.Clear();
        }

        private void ClickExit(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(Lang.LobbyTextExitLobby, Lang.GlobalTextConfirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                this.NavigationService.Navigate(new PlayPage());
            }
        }

        private void ClickStartGame(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new GamePage());
        }
    }
}
