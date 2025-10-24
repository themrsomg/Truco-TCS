using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrucoPrueba1.TrucoServer;

namespace TrucoPrueba1.Views
{
    /// <summary>
    /// Lógica de interacción para LobbyPage.xaml
    /// </summary>
    public partial class LobbyPage : Page
    {
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
            string path = $"pack://application:,,,/TrucoPrueba1;component/Resources/Avatars/{avatarId}.png";
            return new BitmapImage(new System.Uri(path, System.UriKind.Absolute));
        }

        private void InitializeChat()
        {
            AddChatMessage(" ", $"Te uniste a la sala {matchName}");
        }

        private void txtChatMessageTextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlaceholderText != null)
            {
                PlaceholderText.Visibility = string.IsNullOrEmpty(txtChatMessage.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void AddChatMessage(string senderName, string message)
        {
            TextBlock msg = new TextBlock
            {
                Text = senderName == " " ? message : $"{senderName}: {message}",
                TextWrapping = TextWrapping.Wrap,
                Foreground = senderName == " " ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.White,
                FontStyle = senderName == " " ? FontStyles.Italic : FontStyles.Normal,
                Margin = new Thickness(5)
            };
            ChatMessagesPanel.Children.Add(msg);
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
            MessageBoxResult result = MessageBox.Show("¿Deseas salir del lobby?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                this.NavigationService.Navigate(new PreGamePage());
            }
        }

        private void ClickStartGame(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new GamePage());
        }
    }
}
