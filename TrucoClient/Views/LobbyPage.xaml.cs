using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
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
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;
using TrucoClient.Utilities;

namespace TrucoClient.Views
{
    public partial class LobbyPage : Page, IChatPage
    {
        private const string DEFAUL_AVATAR_ID = "avatar_aaa_default";
        private const string MESSAGE_ERROR = "Error";
        private const int FONT_SIZE = 13;
        private readonly string matchCode;
        private readonly string matchName;
        private readonly int maxPlayers;
        private readonly bool isPrivateMatch;
        private bool isOwner = false;

        public LobbyPage(LobbyNavigationArguments arguments)
        {
            InitializeComponent();
            this.matchCode = arguments.MatchCode;
            this.matchName = arguments.MatchName;
            this.maxPlayers = arguments.MaxPlayers;
            this.isPrivateMatch = arguments.IsPrivate;

            txtLobbyTitle.Text = $"Lobby - {this.matchName}";
            txtLobbyCode.Text = string.Format(Lang.GameTextLobbyCode, matchCode);

            InitializeChat();

            this.Loaded += LobbyPage_Loaded;
        }

        private async void ClickStartGame(object sender, RoutedEventArgs e)
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
                await Task.Run(() => ClientManager.MatchClient.StartMatch(matchCode));
            }
            catch (TimeoutException)
            {
                btnStart.IsEnabled = true;
                CustomMessageBox.Show(Lang.ExceptionTextTimeout,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FaultException)
            {
                btnStart.IsEnabled = true;
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (CommunicationException)
            {
                btnStart.IsEnabled = true;
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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

            AddChatMessage(Lang.ChatTextYou, cleanMessage);
            txtChatMessage.Clear();

            try
            {
                ClientManager.MatchClient.SendChatMessage(matchCode, SessionManager.CurrentUsername, originalMessage);
            }
            catch (FaultException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorSendingMessage,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            catch (TimeoutException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextTimeout,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FaultException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorSwitchingTeam,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (CommunicationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorSwitchingTeam, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ClickInviteFriend(object sender, RoutedEventArgs e)
        {
            if (!TryGetButtonContext(sender, out Button btn, out FriendLobbyInfo friendInfo))
            {
                return;
            }

            ToggleInviteButton(btn, friendInfo, false);

            try
            {
                var inviteOptions = CreateInviteOptions(friendInfo.Username);

                bool success = await SendInvitationAsync(inviteOptions);

                if (success)
                {
                    HandleInvitationSuccess(friendInfo);
                }
                else
                {
                    HandleInvitationError(btn, friendInfo, Lang.ExceptionTextErrorSendingInvitation);
                }
            }
            catch (TimeoutException)
            {
                HandleInvitationError(btn, friendInfo, Lang.ExceptionTextTimeout);
            }
            catch (FaultException)
            {
                HandleInvitationError(btn, friendInfo, Lang.ExceptionTextErrorSendingInvitation);
            }
            catch (CommunicationException)
            {
                HandleInvitationError(btn, friendInfo, Lang.ExceptionTextErrorSendingInvitation);
            }
            catch (Exception)
            {
                HandleInvitationError(btn, friendInfo, Lang.ExceptionTextErrorOcurred);
            }
        }

        private static bool TryGetButtonContext(object sender, out Button btn, out FriendLobbyInfo info)
        {
            btn = sender as Button;
            info = btn?.CommandParameter as FriendLobbyInfo;

            return btn != null && info != null && info.CanInvite;
        }

        private static void ToggleInviteButton(Button btn, FriendLobbyInfo info, bool isEnabled)
        {
            btn.IsEnabled = isEnabled;
            info.CanInvite = isEnabled;
        }

        private InviteFriendOptions CreateInviteOptions(string friendUsername)
        {
            return new InviteFriendOptions
            {
                MatchCode = this.matchCode,
                SenderUsername = SessionManager.CurrentUsername,
                FriendUsername = friendUsername
            };
        }

        private static async Task<bool> SendInvitationAsync(InviteFriendOptions options)
        {
            return await Task.Run(() => ClientManager.MatchClient.InviteFriend(options));
        }

        private static void HandleInvitationSuccess(FriendLobbyInfo info)
        {
            CustomMessageBox.Show(string.Format(Lang.DialogTextInvitationSent, info.Username),
                Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);

            info.CanInvite = false;
        }

        private static void HandleInvitationError(Button btn, FriendLobbyInfo info, string messageKey)
        {
            ToggleInviteButton(btn, info, true);
            ShowError(messageKey);
        }

        private static void ShowError(string messageKey)
        {
            Application.Current.Dispatcher.Invoke(() =>
               CustomMessageBox.Show(messageKey, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error));
        }

        private void ClickExit(object sender, RoutedEventArgs e)
        {
            bool? result = CustomMessageBox.Show(Lang.LobbyTextExitLobby, Lang.GlobalTextConfirm, 
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == true)
            {
                try
                {
                    if (!SessionManager.CurrentUsername.StartsWith("Guest_"))
                    {
                        ClientManager.MatchClient.LeaveMatch(matchCode, SessionManager.CurrentUsername);
                    }

                    ClientManager.MatchClient.LeaveMatchChat(matchCode, SessionManager.CurrentUsername);
                }
                catch (TimeoutException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextErrorExitingLobby,
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (FaultException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextErrorExitingLobby,
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (CommunicationException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextErrorExitingLobby,
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextErrorExitingLobby, 
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                this.NavigationService.Navigate(new PlayPage());
            }
        }

        private async Task LoadPlayersAsync()
        {
            try
            {
                var rawPlayers = await FetchPlayersFromService();

                if (rawPlayers == null || rawPlayers.Length == 0)
                {
                    HandleEmptyLobby();
                    return;
                }

                var uiPlayers = await MapServerPlayersToUIAsync(rawPlayers);

                await Application.Current.Dispatcher.InvokeAsync(() => UpdateLobbyUI(uiPlayers));
            }
            catch (TimeoutException)
            {
                ShowError(Lang.ExceptionTextErrorLoadingPlayers);
            }
            catch (FaultException)
            {
                ShowError(Lang.ExceptionTextErrorLoadingPlayers);
            }
            catch (CommunicationException)
            {
                ShowError(Lang.ExceptionTextErrorLoadingPlayers);
            }
            catch (Exception)
            {
                ShowError(Lang.ExceptionTextErrorLoadingPlayers);
            }
        }

        private async Task<PlayerInfo[]> FetchPlayersFromService()
        {
            return await Task.Run(() => ClientManager.MatchClient.GetLobbyPlayers(matchCode));
        }

        private void HandleEmptyLobby()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddChatMessage(string.Empty, Lang.LobbyTextNoPlayersYet);
                PlayersList.ItemsSource = new List<PlayerLobbyInfo>();
                btnStart.IsEnabled = false;
            });
        }

        private async Task<List<PlayerLobbyInfo>> MapServerPlayersToUIAsync(PlayerInfo[] players)
        {
            var tasks = players.Select(p => GetSinglePlayerInfoAsync(p));
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        private void UpdateLobbyUI(List<PlayerLobbyInfo> players)
        {
            PlayersList.ItemsSource = null;
            PlayersList.ItemsSource = players;

            isOwner = players.Any(p => p.OwnerUsername == SessionManager.CurrentUsername);

            UpdateStartButtonState(players.Count);
            UpdateFriendsPanelState(players);
        }

        private void UpdateStartButtonState(int currentPlayerCount)
        {
            btnStart.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;

            bool isLobbyFull = currentPlayerCount == this.maxPlayers;
            btnStart.IsEnabled = isOwner && isLobbyFull;
        }

        private void UpdateFriendsPanelState(List<PlayerLobbyInfo> currentPlayers)
        {
            if (isOwner && isPrivateMatch)
            {
                pnlFriendsContainer.Visibility = Visibility.Visible;
                _ = LoadFriendsForInvite(currentPlayers);
            }
            else
            {
                pnlFriendsContainer.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadFriendsForInvite(List<PlayerLobbyInfo> currentPlayers)
        {
            if (!isOwner)
            { 
                return; 
            }

            try
            {
                var allFriends = await FetchAllFriends();

                if (allFriends == null)
                {
                    return;
                } 

                var inviteableFriends = FilterFriendsNotInLobby(allFriends, currentPlayers);

                FriendsList.ItemsSource = inviteableFriends;
            }
            catch (Exception)
            {
                /*
                 * Loading inviteable friends is a non-essential 
                 * feature for the lobby's core functionality.
                 * If fetching or filtering friends fails,
                 * we silently ignore it to prevent disrupting 
                 * the user experience or causing the app to crash.
                 * The friends list will simply remain empty or unchanged, 
                 * allowing the match to proceed without this optional UI element.
                 */
            }
        }

        private static async Task<FriendData[]> FetchAllFriends()
        {
            return await Task.Run(() =>
                ClientManager.FriendClient.GetFriends(SessionManager.CurrentUsername));
        }

        private List<FriendLobbyInfo> FilterFriendsNotInLobby(FriendData[] friends, List<PlayerLobbyInfo> lobbyPlayers)
        {
            return friends
                .Where(f => !lobbyPlayers.Any(p => p.Username.Equals(f.Username, StringComparison.OrdinalIgnoreCase)))
                .Select(f => new FriendLobbyInfo
                {
                    Username = f.Username,
                    AvatarUri = LoadAvatar(f.AvatarId),
                    CanInvite = true
                })
                .ToList();
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

        private async void LobbyPage_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeBannedWordsAsync();
            await JoinChatAndLoadPlayersAsync();
        }

        public void ReloadPlayersDeferred()
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(250);
                await LoadPlayersAsync();
            });
        }

        private static async Task InitializeBannedWordsAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var bannedList = ClientManager.MatchClient.GetBannedWords();
                        ProfanityValidator.Instance.Initialize(bannedList);
                    }
                    catch
                    {
                        /*
                         * Non-critical feature: Profanity filtering is optional 
                         * and not essential to core lobby functionality. 
                         * Silently fail to avoid disrupting the user 
                         * experience or main application flow.
                         */
                    }
                });
            }
            catch
            {
                /*
                 * Handles potential errors in task creation or execution 
                 * (threading issues). Kept silent as this is a background 
                 * initialization that should not interrupt the primary 
                 * lobby loading process.
                 */
            }
        }

        private async Task JoinChatAndLoadPlayersAsync()
        {
            try
            {
                if (!SessionManager.CurrentUsername.StartsWith("Guest_"))
                {
                    await Task.Delay(200);
                }

                await Task.Run(() =>
                    ClientManager.MatchClient.JoinMatchChat(matchCode, SessionManager.CurrentUsername));

                await Task.Delay(100);
                await LoadPlayersAsync();
            }
            catch
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    CustomMessageBox.Show(Lang.ExceptionTextUnableConnectChat, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Warning));
            }
        }

        private static BitmapImage LoadAvatar(string avatarId)
        {
            string path = $"pack://application:,,,/Resources/Avatars/{avatarId}.png";

            try
            {
                return CreateImage(path);
            }
            catch (ArgumentNullException)
            {
                return LoadDefaultAvatar();
            }
            catch (UriFormatException)
            {
                return LoadDefaultAvatar();
            }
            catch (IOException)
            {
                return LoadDefaultAvatar();
            }
            catch (Exception)
            {
                return LoadDefaultAvatar();
            }
        }

        private static BitmapImage LoadDefaultAvatar()
        {
            try
            {
                return CreateImage(ResourcePaths.DEFAULT_AVATAR_PATH);
            }
            catch
            {
                return null;
            }
        }

        private static BitmapImage CreateImage(string path)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Absolute);

            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.EndInit();

            if (image.CanFreeze)
            {
                image.Freeze();
            }

            return image;
        }

        private void InitializeChat()
        {
            AddChatMessage(string.Empty, string.Format(Lang.LobbyTextJoinedRoom, matchCode));
        }

        private async Task<PlayerLobbyInfo> GetSinglePlayerInfoAsync(PlayerInfo playerInfo)
        {
            if (playerInfo.Username.StartsWith("Guest_"))
            {
                return CreateFallbackInfo(playerInfo);
            }

            try
            {
                var profile = await ClientManager.UserClient.GetUserProfileAsync(playerInfo.Username);

                string avatarId = string.IsNullOrEmpty(profile?.AvatarId) ? DEFAUL_AVATAR_ID : profile.AvatarId;
                string username = profile?.Username ?? playerInfo.Username;

                return new PlayerLobbyInfo
                {
                    Username = username,
                    AvatarUri = LoadAvatar(avatarId),
                    Team = playerInfo.Team,
                    IsCurrentUser = username == SessionManager.CurrentUsername,
                    OwnerUsername = playerInfo.OwnerUsername
                };
            }
            catch (TimeoutException)
            {
                return CreateFallbackInfo(playerInfo);
            }
            catch (FaultException)
            {
                return CreateFallbackInfo(playerInfo);
            }
            catch (CommunicationException)
            {
                return CreateFallbackInfo(playerInfo);
            }
            catch (Exception)
            {
                return CreateFallbackInfo(playerInfo);
            }
        }

        PlayerLobbyInfo CreateFallbackInfo(PlayerInfo playerInfo)
        {
            return new PlayerLobbyInfo
            {
                Username = playerInfo.Username,
                AvatarUri = LoadAvatar(DEFAUL_AVATAR_ID),
                Team = playerInfo.Team,
                IsCurrentUser = playerInfo.Username == SessionManager.CurrentUsername,
                OwnerUsername = playerInfo.OwnerUsername
            };
        }
    }
}
