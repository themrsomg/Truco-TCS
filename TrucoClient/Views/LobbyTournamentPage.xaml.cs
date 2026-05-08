using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrucoClient.Helpers.DTOs;
using TrucoClient.Helpers.Paths;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Helpers.UI;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class LobbyTournamentPage : Page, ITrucoTournamentCallback
    {
        private const int MAX_CHAT_CHARS = 200;
        private readonly int currentTournamentId;
        private ObservableCollection<PlayerDisplay> participants;

        public LobbyTournamentPage(int tournamentId)
        {
            InitializeComponent();
            this.currentTournamentId = tournamentId;
            this.participants = new ObservableCollection<PlayerDisplay>();

            LobbyPlayersList.ItemsSource = this.participants;
            InputRestriction.AttachChatValidation(this.txtChatMessage, MAX_CHAT_CHARS);

            ClientManager.SetCallbackHandler(null);
        }

        public void OnTournamentPlayerJoined(string username, int currentCapacity)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var newPlayer = new PlayerDisplay
                {
                    Username = username,
                    AvatarUri = LoadDefaultAvatar()
                };

                this.participants.Add(newPlayer);
                this.txtPlayerCount.Text = $"{currentCapacity} jugadores";

                AddSystemMessage($"{username} se ha unido al torneo.");
            });
        }

        public void OnTournamentStarted(List<BracketDTO> tree)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.NavigationService.Navigate(new TournamentBracketsPage(this.currentTournamentId, tree));
            });
        }

        public void OnBracketUpdated(BracketDTO updatedBracket)
        {
        }

        private void ClickSendMessage(object sender, RoutedEventArgs e)
        {
            string message = txtChatMessage.Text.Trim();

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            AddUserMessage(SessionManager.CurrentUsername, message);
            txtChatMessage.Clear();
        }

        private void AddSystemMessage(string message)
        {
            TextBlock messageText = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 16,
                Text = message,
                Foreground = Brushes.DarkGray,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 0, 0, 5)
            };

            ChatMessagesPanel.Children.Add(messageText);
            ChatScroll.ScrollToEnd();
        }

        private void AddUserMessage(string senderName, string message)
        {
            TextBlock messageText = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 18,
                Text = $"{senderName}: {message}",
                Foreground = Brushes.Black,
                Margin = new Thickness(0, 0, 0, 5)
            };

            ChatMessagesPanel.Children.Add(messageText);
            ChatScroll.ScrollToEnd();
        }

        private void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ClickSendMessage(btnSendMessage, null);
                e.Handled = true;
            }
        }

        private BitmapImage LoadDefaultAvatar()
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(ResourcePaths.DEFAULT_AVATAR_PATH, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }

    public class PlayerDisplay
    {
        public string Username { get; set; }
        public BitmapImage AvatarUri { get; set; }
    }
}