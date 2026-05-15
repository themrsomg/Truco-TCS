using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        private const string MESSAGE_ERROR = "Error";
        private readonly string tournamentCode;
        private readonly bool isHost;
        private ObservableCollection<PlayerDisplay> participants;

        public LobbyTournamentPage(string code, bool isHost)
        {
            InitializeComponent();
            this.tournamentCode = code;
            this.isHost = isHost;
            this.participants = new ObservableCollection<PlayerDisplay>();

            LobbyPlayersList.ItemsSource = this.participants;
            InputRestriction.AttachChatValidation(this.txtChatMessage, MAX_CHAT_CHARS);

            ClientManager.SetCallbackHandler(null);

            txtTournamentCode.Text = $"Código: {code}";
            btnStartTournament.Visibility = isHost ? Visibility.Visible : Visibility.Collapsed;

            if (isHost)
            {
                participants.Add(new PlayerDisplay
                {
                    Username = SessionManager.CurrentUsername,
                    AvatarUri = LoadDefaultAvatar()
                });
                txtPlayerCount.Text = "1 jugadores";
            }
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

        public void OnTournamentPlayerLeft(string username, int currentCapacity)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var player = this.participants.FirstOrDefault(p => p.Username == username);
                if (player != null) this.participants.Remove(player);
                this.txtPlayerCount.Text = $"{currentCapacity} jugadores";
                AddSystemMessage($"{username} salió del torneo.");
            });
        }

        public void OnTournamentStarted(List<BracketDTO> tree)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.NavigationService.Navigate(new TournamentBracketsPage(tree));
            });
        }

        public void OnTournamentCancelled(string reason)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CustomMessageBox.Show(reason, "Torneo cancelado", MessageBoxButton.OK, MessageBoxImage.Information);
                this.NavigationService.Navigate(new PlayPage());
            });
        }

        public void OnBracketUpdated(BracketDTO updatedBracket)
        {
        }

        private async void ClickStartTournament(object sender, RoutedEventArgs e)
        {
            btnStartTournament.IsEnabled = false;
            try
            {
                int userId = SessionManager.CurrentUserData.PlayerId;
                bool success = await Task.Run(() => ClientManager.TournamentClient.StartTournament(this.tournamentCode, userId));

                if (!success)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CustomMessageBox.Show("No se puede iniciar. Verifica que el torneo esté lleno.", MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                        btnStartTournament.IsEnabled = true;
                    });
                }
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CustomMessageBox.Show("Error al iniciar el torneo.", MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    btnStartTournament.IsEnabled = true;
                });
            }
        }

        private async void ClickLeaveTournament(object sender, RoutedEventArgs e)
        {
            try
            {
                int userId = SessionManager.CurrentUserData.PlayerId;
                await Task.Run(() => ClientManager.TournamentClient.LeaveTournament(this.tournamentCode, userId));
            }
            catch (Exception)
            {
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.NavigationService.Navigate(new TournamentMenuPage());
            });
        }

        private void ClickSendMessage(object sender, RoutedEventArgs e)
        {
            string message = txtChatMessage.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;
            AddUserMessage(SessionManager.CurrentUsername, message);
            txtChatMessage.Clear();
        }

        private void AddSystemMessage(string message)
        {
            ChatMessagesPanel.Children.Add(new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 16,
                Text = message,
                Foreground = Brushes.DarkGray,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 0, 0, 5)
            });
            ChatScroll.ScrollToEnd();
        }

        private void AddUserMessage(string senderName, string message)
        {
            ChatMessagesPanel.Children.Add(new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 18,
                Text = $"{senderName}: {message}",
                Foreground = Brushes.Black,
                Margin = new Thickness(0, 0, 0, 5)
            });
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
            image.UriSource = new Uri($"pack://application:,,,{ResourcePaths.DEFAULT_AVATAR_PATH}", UriKind.Absolute);
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