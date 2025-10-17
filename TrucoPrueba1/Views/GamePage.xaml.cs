using System;
using System.Collections.Generic;
using System.Linq;
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
using TrucoPrueba1.Properties.Langs;

namespace TrucoPrueba1
{
    /// <summary>
    /// Lógica de interacción para GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        public GamePage()
        {
            InitializeComponent();
            DataContext = new TrucoPrueba1.TrucoServer.UserProfileData
            {
                AvatarId = SessionManager.CurrentUserData?.AvatarId ?? "avatar_aaa_default"
            };

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
                Text = $"{senderName}: {message}",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13
            };

            messageBubble.Child = messageText;

            ChatMessagesPanel.Children.Add(messageBubble);
        }

        private void txtChatMessageTextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlaceholderText != null)
                PlaceholderText.Visibility = string.IsNullOrEmpty(txtChatMessage.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }
        private void ClickSendMessage(object sender, RoutedEventArgs e)
        {
            string messageText = txtChatMessage.Text.Trim();
            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }

            AddChatMessage("Tú", messageText); ///////CAMBIAR

            txtChatMessage.Clear();

            // SessionManager.MatchClient.SendChatMessage(currentMatchId, currentPlayer, messageText);
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
                AddMessageToChat($"Tú: {emoji}"); ////CAMBIAR
            }
        }

        private void AddMessageToChat(string message)
        {
            TextBlock msg = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5),
                FontSize = 13
            };
            ChatMessagesPanel.Children.Add(msg);
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
                this.NavigationService.Navigate(new MainPage());
            }
        }
    }
}
