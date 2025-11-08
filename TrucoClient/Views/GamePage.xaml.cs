using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient
{
    public partial class GamePage : Page
    {
        private readonly string matchCode;
        private string currentPlayer => SessionManager.CurrentUsername;

        public GamePage(string matchCode, List<PlayerInfo> players)
        {
            InitializeComponent();
            this.matchCode = matchCode;

            LoadPlayerAvatars(players);

            try
            {
                var matchClient = ClientManager.MatchClient;
                matchClient.JoinMatchChat(this.matchCode, SessionManager.CurrentUsername);
            }
            catch (Exception ex)
            {
                MessageBox.Show( string.Format(Lang.ExceptionTextUnableConnectChat, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClickSendMessage(object sender, RoutedEventArgs e)
        {
            string messageText = txtChatMessage.Text.Trim();
            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }

            AddChatMessage(Lang.ChatTextYou, messageText);
            txtChatMessage.Clear();

            try
            {
                var matchClient = ClientManager.MatchClient;
                matchClient.SendChatMessage(this.matchCode, currentPlayer, messageText);
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                try
                {
                    var matchClient = ClientManager.MatchClient;
                    matchClient.SendChatMessage(this.matchCode, currentPlayer, emoji);
                }
                catch (CommunicationException ex)
                {
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show( string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ClickBack(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show( Lang.GameTextExitGameConfirmation, Lang.GlobalTextConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var matchClient = ClientManager.MatchClient;
                    matchClient.LeaveMatchChat(this.matchCode, SessionManager.CurrentUsername);
                    this.NavigationService.Navigate(new MainPage());
                }
                catch (CommunicationException ex)
                {
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorExitingLobby, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == txtChatMessage)
                {
                    ClickSendMessage(btnSendMessage, null);
                    e.Handled = true;
                }
            }
        }

        private void AddChatMessage(string senderName, string message)
        {
            Border messageBubble = new Border
            {
                Padding = new Thickness(5),
                Margin = new Thickness(2)
            };

            TextBlock messageText = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13
            };

            if (string.IsNullOrEmpty(senderName))
            {
                messageText.Text = message;
                messageText.Foreground = Brushes.DarkGray;
                messageText.FontStyle = FontStyles.Italic;
            }
            else
            {
                messageText.Text = $"{senderName}: {message}";
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

        public void ReceiveChatMessage(string senderName, string message)
        {
            AddChatMessage(senderName, message);
        }

        private BitmapImage LoadAvatar(string avatarId)
        {
            if (string.IsNullOrWhiteSpace(avatarId))
            {
                avatarId = "avatar_aaa_default";
            }

            string packUri = $"pack://application:,,,/TrucoClient;component/Resources/Avatars/{avatarId}.png";
            try
            {
                return new BitmapImage(new Uri(packUri, UriKind.Absolute));
            }
            catch
            {
                return new BitmapImage(new Uri("pack://application:,,,/TrucoClient;component/Resources/Avatars/avatar_aaa_default.png", UriKind.Absolute));
            }
        }

        private void LoadPlayerAvatars(List<PlayerInfo> players)
        {
            try
            {
                var currentUsername = SessionManager.CurrentUsername;
                var current = players.FirstOrDefault(p => p.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase));
                var rival = players.FirstOrDefault(p => !p.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase));

                if (current != null)
                {
                    imgPlayerAvatar.Source = LoadAvatar(current.AvatarId);
                }

                if (rival != null)
                {
                    imgRivalAvatar.Source = LoadAvatar(rival.AvatarId);
                }
                else
                {
                    imgRivalAvatar.Source = LoadAvatar("avatar_aaa_default");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading avatars: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
