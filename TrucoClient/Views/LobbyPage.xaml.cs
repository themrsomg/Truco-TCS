using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;

namespace TrucoClient.Views
{
    public partial class LobbyPage : Page
    {
        private const string DEFAUL_AVATAR_ID = "avatar_aaa_default";
        private const string MESSAGE_ERROR = "Error";
        private const string DEFAULT_AVATAR_PATH = "/Resources/Avatars/avatar_aaa_default.png";
        private const int FONT_SIZE = 13;
        private readonly string matchCode;
        private readonly string matchName;
        private bool isOwner = false;

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

            txtLobbyTitle.Text = $"Lobby - {this.matchName}";
            txtLobbyCode.Text = string.Format(Lang.GameTextLobbyCode, matchCode);

            _ = LoadPlayersAsync();
            InitializeChat();

            _ = Task.Delay(200).ContinueWith(_ => Application.Current.Dispatcher.Invoke(async () => await LoadPlayersAsync()));

            this.Loaded += LobbyPage_Loaded;
        }

        private void ClickStartGame(object sender, RoutedEventArgs e)
        {
            if (!isOwner)
            {
                MessageBox.Show(Lang.GameTextNotOwnerStartGame, Lang.GlobalTextAccessDenied, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Task.Run(() => ClientManager.MatchClient.StartMatch(matchCode));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.GameTextErrorStartingMatch, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickSendMessage(object sender, RoutedEventArgs e)
        {
            string text = txtChatMessage.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            ClientManager.MatchClient.SendChatMessage(matchCode, SessionManager.CurrentUsername, text);
            AddChatMessage(SessionManager.CurrentUsername, text);
            txtChatMessage.Clear();
        }

        private void ClickExit(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                Lang.LobbyTextExitLobby,
                Lang.GlobalTextConfirm,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ClientManager.MatchClient.LeaveMatch(matchCode, SessionManager.CurrentUsername);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorExitingLobby, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                this.NavigationService.Navigate(new PlayPage());
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

        private void ChatMessageTextChanged(object sender, TextChangedEventArgs e)
        {
            if (blckPlaceholder != null)
            {
                blckPlaceholder.Visibility = string.IsNullOrEmpty(txtChatMessage.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public void AddChatMessage(string senderName, string message)
        {
            TextBlock messageText = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = FONT_SIZE
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

            ChatMessagesPanel.Children.Add(messageText);
        }

        private void LobbyPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Task.Run(() => {
                    try
                    {
                        ClientManager.MatchClient.JoinMatchChat(matchCode, SessionManager.CurrentUsername);
                    }
                    catch
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                            MessageBox.Show(Lang.ExceptionTextUnableConnectChat, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning));
                    }
                });
            }
            catch 
            {
                /* 
                 * Intentionally empty exception handler.
                 * Any errors that might occur while trying to start 
                 * the background task are ignored, as actual error 
                 * handling is done within Task.Run.
                 */
            }
        }

        private async Task LoadPlayersAsync()
        {
            try
            {
                var players = await Task.Run(() => ClientManager.MatchClient.GetLobbyPlayers(matchCode));

                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    if (players == null || players.Length == 0)
                    {
                        AddChatMessage(string.Empty, Lang.LobbyTextNoPlayersYet);
                        PlayersList.ItemsSource = new List<PlayerLobbyInfo>();
                        return;
                    }

                    PlayersList.ItemsSource = null;

                    var playerLoadingTasks = players.Select(p => GetSinglePlayerInfoAsync(p)).ToList();
                    var playerInfos = (await Task.WhenAll(playerLoadingTasks)).ToList();

                    PlayersList.ItemsSource = playerInfos;

                    isOwner = players.Any(p => p.OwnerUsername == SessionManager.CurrentUsername);
                    btnStart.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    MessageBox.Show(string.Format(Lang.ExceptionTextErrorLoadingPlayers, ex.Message), MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        public void ReloadPlayersDeferred()
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(250);
                await LoadPlayersAsync();
            });
        }

        private static BitmapImage LoadAvatar(string avatarId)
        {
            try
            {
                string path = $"/Resources/Avatars/{avatarId}.png";
                return new BitmapImage(new Uri(path, UriKind.Relative));
            }
            catch
            {
                return new BitmapImage(new Uri(DEFAULT_AVATAR_PATH, UriKind.Relative));
            }
        }

        private void InitializeChat()
        {
            AddChatMessage(string.Empty, string.Format(Lang.LobbyTextJoinedRoom, matchCode));
        }

        private async Task<PlayerLobbyInfo> GetSinglePlayerInfoAsync(TrucoServer.PlayerInfo p)
        {
            if (p.Username.StartsWith("Guest_"))
            {
                return new PlayerLobbyInfo
                {
                    Username = p.Username,
                    AvatarUri = LoadAvatar(DEFAUL_AVATAR_ID)
                };
            }

            try
            {
                var profile = await ClientManager.UserClient.GetUserProfileAsync(p.Username);
                string avatarId = string.IsNullOrEmpty(profile?.AvatarId) ? DEFAUL_AVATAR_ID : profile.AvatarId;

                return new PlayerLobbyInfo
                {
                    Username = profile?.Username ?? p.Username,
                    AvatarUri = LoadAvatar(avatarId)
                };
            }
            catch (Exception)
            {
                return new PlayerLobbyInfo
                {
                    Username = p.Username,
                    AvatarUri = LoadAvatar(DEFAUL_AVATAR_ID)
                };
            }
        }
    }
}
