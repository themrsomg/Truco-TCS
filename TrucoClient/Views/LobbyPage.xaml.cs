using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Properties.Langs;
using TrucoClient.Utilities;

namespace TrucoClient.Views
{
    public partial class LobbyPage : Page, IChatPage
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
            public string Team { get; set; }
            public bool IsCurrentUser { get; set; }
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

            _ = Task.Delay(200).ContinueWith(_ 
                => Application.Current.Dispatcher.Invoke(
                    async () => await LoadPlayersAsync())
                );

            this.Loaded += LobbyPage_Loaded;
        }

        private void ClickStartGame(object sender, RoutedEventArgs e)
        {
            if (!isOwner)
            {
                CustomMessageBox.Show(Lang.GameTextNotOwnerStartGame, Lang.GlobalTextAccessDenied, 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                
                return;
            }

            btnStart.IsEnabled = false;

            try
            {
                Task.Run(() => ClientManager.MatchClient.StartMatch(matchCode));
            }
            catch (Exception)
            {
                btnStart.IsEnabled = true;
                CustomMessageBox.Show(Lang.GameTextErrorStartingMatch, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickSendMessage(object sender, RoutedEventArgs e)
        {
            string originalMessage = txtChatMessage.Text.Trim();
            
            if (string.IsNullOrEmpty(originalMessage))
            {
                return;
            }

            string cleanMessage = ProfanityValidator.Instance.CensorText(originalMessage);

            ClientManager.MatchClient.SendChatMessage(matchCode, SessionManager.CurrentUsername, cleanMessage);
            AddChatMessage(SessionManager.CurrentUsername, cleanMessage);
            txtChatMessage.Clear();
        }

        private async void ClickSwitchTeam(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button switchButton) || !(switchButton.CommandParameter is string usernameToSwitch))
            {
                return;
            }

            if (!isOwner && usernameToSwitch != SessionManager.CurrentUsername)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCannotSwitchOthersTeam, Lang.GlobalTextAccessDenied, 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
               
                return;
            }

            try
            {
                await Task.Run(() => ClientManager.MatchClient.SwitchTeam(this.matchCode, usernameToSwitch));
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorSwitchingTeam, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickExit(object sender, RoutedEventArgs e)
        {
            bool? result = CustomMessageBox.Show(Lang.LobbyTextExitLobby, Lang.GlobalTextConfirm, 
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == true)
            {
                try
                {
                    ClientManager.MatchClient.LeaveMatch(matchCode, SessionManager.CurrentUsername);
                }
                catch (Exception)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextErrorExitingLobby, 
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
                Task.Run(() =>
                {
                    try
                    {
                        var bannedList = ClientManager.MatchClient.GetBannedWords();

                        ProfanityValidator.Instance.Initialize(bannedList);
                    }
                    catch
                    {
                        /* 
                         * Silently ignore if banned words fail to load - 
                         * the feature is non-critical and we don't want to 
                         * disrupt the user experience with error messages
                         */
                    }
                });

                Task.Run(() => {
                    try
                    {
                        ClientManager.MatchClient.JoinMatchChat(matchCode, SessionManager.CurrentUsername);
                    }
                    catch
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                            CustomMessageBox.Show(Lang.ExceptionTextUnableConnectChat, MESSAGE_ERROR, 
                                MessageBoxButton.OK, MessageBoxImage.Warning));
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
            catch (Exception)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    CustomMessageBox.Show(Lang.ExceptionTextErrorLoadingPlayers, 
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error));
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
                    AvatarUri = LoadAvatar(DEFAUL_AVATAR_ID),
                    Team = p.Team,
                    IsCurrentUser = p.Username == SessionManager.CurrentUsername
                };
            }

            try
            {
                var profile = await ClientManager.UserClient.GetUserProfileAsync(p.Username);
               
                string avatarId = string.IsNullOrEmpty(profile?.AvatarId) ? DEFAUL_AVATAR_ID : profile.AvatarId;
                string username = profile?.Username ?? p.Username;

                return new PlayerLobbyInfo
                {
                    Username = profile?.Username ?? p.Username,
                    AvatarUri = LoadAvatar(avatarId),
                    Team = p.Team,
                    IsCurrentUser = username == SessionManager.CurrentUsername
                };
            }
            catch (Exception)
            {
                return new PlayerLobbyInfo
                {
                    Username = p.Username,
                    AvatarUri = LoadAvatar(DEFAUL_AVATAR_ID),
                    Team = p.Team,
                    IsCurrentUser = p.Username == SessionManager.CurrentUsername
                };
            }
        }
    }
}
