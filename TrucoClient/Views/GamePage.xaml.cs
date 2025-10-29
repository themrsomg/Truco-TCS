using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrucoClient.Properties.Langs;

namespace TrucoClient
{
    public partial class GamePage : Page
    {
        public GamePage()
        {
            InitializeComponent();
            string avatarId = SessionManager.CurrentUserData?.AvatarId ?? "avatar_aaa_default";
            LoadAvatarImage(avatarId);

            var matchClient = ClientManager.MatchClient;
            matchClient.JoinMatchChat("SalaTruco001", SessionManager.CurrentUsername);
        }

        private string currentMatchId = "SalaTruco001"; //////TODO Hay que generar codigo especial para cada partida
        private string currentPlayer => SessionManager.CurrentUsername;
        private void AddChatMessage(string senderName, string message)
        {
            Border messageBubble = new Border
            {
                Padding = new Thickness(5),
                Margin = new Thickness(2)
            };

            TextBlock messageText = new TextBlock();

            if (senderName.Equals(" "))
            {
                messageText = new TextBlock
                {
                    Text = $"{message}",
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 13
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
                    FontSize = 13
                };
            }

            messageBubble.Child = messageText;
            ChatMessagesPanel.Children.Add(messageBubble);
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
        private void ClickSendMessage(object sender, RoutedEventArgs e) /////////////TODO agregar filtros de chat
        {
            string messageText = txtChatMessage.Text.Trim();
            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }

            AddChatMessage(Lang.ChatTextYou, messageText);

            txtChatMessage.Clear();

            var matchClient = ClientManager.MatchClient;
            matchClient.SendChatMessage(currentMatchId, currentPlayer, messageText);
        }

        private void ClickOpenGesturesMenu(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }
        private void ClickGesture(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                string emoji = item.Header.ToString();
                AddChatMessage(Lang.ChatTextYou, emoji);

                var matchClient = ClientManager.MatchClient;
                matchClient.SendChatMessage(currentMatchId, currentPlayer, emoji);
            }
        }
        public void ReceiveChatMessage(string senderName, string message)
        {
            AddChatMessage(senderName, message);
        }


        private void ClickBack(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                Lang.GameTextExitGameConfirmation,
                Lang.GlobalTextConfirmation,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                var matchClient = ClientManager.MatchClient;
                matchClient.LeaveMatchChat("SalaTruco001", SessionManager.CurrentUsername);
                this.NavigationService.Navigate(new MainPage());
            }
        }

        private void LoadAvatarImage(string avatarId)
        {
            if (string.IsNullOrWhiteSpace(avatarId))
            {
                avatarId = "avatar_aaa_default";
            }

            string packUri = $"pack://application:,,,/TrucoClient;component/Resources/Avatars/{avatarId}.png";

            try
            {
                imgPlayerAvatar.Source = new BitmapImage(new Uri(packUri, UriKind.Absolute));
            }
            catch
            {
                imgPlayerAvatar.Source = new BitmapImage(new Uri("pack://application:,,,/TrucoClient;component/Resources/Avatars/avatar_aaa_default.png", UriKind.Absolute));
            }
        }
    }
}
