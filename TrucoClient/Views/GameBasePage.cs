using System;
using System.Collections.Generic;
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
    public abstract class GameBasePage : Page
    {
        protected const string DEFAULT_AVATAR_ID = "avatar_aaa_default";
        protected const string MESSAGE_ERROR = "Error";
        protected const string DEFAULT_AVATAR_PATH = "/Resources/Avatars/avatar_aaa_default.png";

        protected string MatchCode;
        protected static string CurrentPlayer => SessionManager.CurrentUsername;

        protected TextBox TxtChatMessage;
        protected Panel ChatMessagesPanel;
        protected TextBlock BlckPlaceholder;

        protected void InitializeBase(string matchCode, TextBox txtChatMessage, Panel chatMessagesPanel, TextBlock blckPlaceholder)
        {
            this.MatchCode = matchCode;
            this.TxtChatMessage = txtChatMessage;
            this.ChatMessagesPanel = chatMessagesPanel;
            this.BlckPlaceholder = blckPlaceholder;

            ConnectToChat();
        }

        private void ConnectToChat()
        {
            try
            {
                var matchClient = ClientManager.MatchClient;
                matchClient.JoinMatchChat(this.MatchCode, SessionManager.CurrentUsername);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextUnableConnectChat, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        protected void ClickSendMessage(object sender, RoutedEventArgs e)
        {
            string messageText = TxtChatMessage.Text.Trim();
            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }

            AddChatMessage(Lang.ChatTextYou, messageText);
            TxtChatMessage.Clear();

            try
            {
                ClientManager.MatchClient.SendChatMessage(this.MatchCode, CurrentPlayer, messageText);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void ClickOpenGesturesMenu(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }

        protected void ClickGesture(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                string emoji = item.Header.ToString();
                AddChatMessage(Lang.ChatTextYou, emoji);
                try
                {
                    ClientManager.MatchClient.SendChatMessage(this.MatchCode, CurrentPlayer, emoji);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorSendingMessage, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected void ClickBack(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(Lang.GameTextExitGameConfirmation, Lang.GlobalTextConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ClientManager.MatchClient.LeaveMatchChat(this.MatchCode, SessionManager.CurrentUsername);
                    this.NavigationService.Navigate(new MainPage());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorExitingLobby, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender == TxtChatMessage)
            {
                ClickSendMessage(sender, null);
                e.Handled = true;
            }
        }

        protected void AddChatMessage(string senderName, string message)
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

        protected void ChatMessageTextChanged(object sender, TextChangedEventArgs e)
        {
            if (BlckPlaceholder != null && TxtChatMessage != null)
            {
                BlckPlaceholder.Visibility = string.IsNullOrEmpty(TxtChatMessage.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public void ReceiveChatMessage(string senderName, string message)
        {
            AddChatMessage(senderName, message);
        }

        protected static BitmapImage LoadAvatar(string avatarId)
        {
            if (string.IsNullOrWhiteSpace(avatarId))
            {
                avatarId = DEFAULT_AVATAR_ID;
            }
            try
            {
                return new BitmapImage(new Uri($"/Resources/Avatars/{avatarId}.png", UriKind.Relative));
            }
            catch
            {
                return new BitmapImage(new Uri(DEFAULT_AVATAR_PATH, UriKind.Relative));
            }
        }

        protected abstract void LoadPlayerAvatars(List<PlayerInfo> players);
    }
}