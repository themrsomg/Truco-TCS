using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;

namespace TrucoClient.Views
{
    public partial class GameFourPlayersPage : Page
    {
        private const string DEFAUL_AVATAR_ID = "avatar_aaa_default";
        private const string MESSAGE_ERROR = "Error";
        private const string DEFAULT_AVATAR_PATH = "/Resources/Avatars/avatar_aaa_default.png";
        private readonly string matchCode;
        private static string currentPlayer => SessionManager.CurrentUsername;

        public GameFourPlayersPage(string matchCode, List<PlayerInfo> players)
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
                MessageBox.Show(string.Format(Lang.ExceptionTextUnableConnectChat, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(Lang.GameTextExitGameConfirmation, Lang.GlobalTextConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);

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
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorExitingLobby, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender == txtChatMessage)
            {
                ClickSendMessage(btnSendMessage, null);
                e.Handled = true;
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
            if (VisualTreeHelper.GetParent(ChatMessagesPanel) is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToBottom();
            }
        }

        private void ChatMessageTextChanged(object sender, TextChangedEventArgs e)
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

        private static BitmapImage LoadAvatar(string avatarId)
        {
            if (string.IsNullOrWhiteSpace(avatarId))
            {
                avatarId = DEFAUL_AVATAR_ID;
            }

            string relativeUri = $"/Resources/Avatars/{avatarId}.png";
            try
            {
                return new BitmapImage(new Uri(relativeUri, UriKind.Relative));
            }
            catch
            {
                return new BitmapImage(new Uri(DEFAULT_AVATAR_PATH, UriKind.Relative));
            }
        }

        private void LoadPlayerAvatars(List<PlayerInfo> players)
        {
            try
            {
                if (players == null || players.Count == 0) return;

                var currentUsername = SessionManager.CurrentUsername;
                int selfIndex = players.FindIndex(p => p.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase));

                if (selfIndex != -1)
                {
                    imgPlayerAvatar.Source = LoadAvatar(players[selfIndex].AvatarId);

                    if (players.Count >= 4)
                    {

                        var leftPlayer = players[(selfIndex + 1) % 4];
                        var topPlayer = players[(selfIndex + 2) % 4];
                        var rightPlayer = players[(selfIndex + 3) % 4];

                        imgLeftAvatar.Source = LoadAvatar(leftPlayer.AvatarId);
                        imgTopAvatar.Source = LoadAvatar(topPlayer.AvatarId);
                        imgRightAvatar.Source = LoadAvatar(rightPlayer.AvatarId);
                    }
                    else
                    {
                        var rivals = players.Where(p => !p.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase)).ToList();
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