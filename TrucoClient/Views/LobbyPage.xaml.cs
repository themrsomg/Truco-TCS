    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using TrucoClient.Properties.Langs;
    using TrucoClient.TrucoServer;

    namespace TrucoClient.Views
    {
        public partial class LobbyPage : Page
        {
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

                txtLobbyTitle.Text = $"Lobby - {matchName}";
                txtLobbyCode.Text = string.Format(Lang.GameTextLobbyCode, matchCode);

                var currentUser = SessionManager.CurrentUserData;
                if (currentUser != null)
                {
                    txtLobbyTitle.Text = $"Lobby - {matchName} ({currentUser.Username})";
                }

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
                    MessageBox.Show(string.Format(Lang.GameTextErrorStartingMatch, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show(string.Format(Lang.ExceptionTextErrorExitingLobby, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    this.NavigationService.Navigate(new PlayPage());
                }
            }

            private void txtChatMessageTextChanged(object sender, TextChangedEventArgs e)
            {
                blckPlaceholder.Visibility = string.IsNullOrEmpty(txtChatMessage.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
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
                                MessageBox.Show(Lang.ExceptionTextUnableConnectChat, "Error", MessageBoxButton.OK, MessageBoxImage.Warning));
                        }
                    });
                }
                catch 
                { 
                 
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

                        var playerInfos = new List<PlayerLobbyInfo>();
                        foreach (var p in players)
                        {
                            try
                            {
                                var profile = await ClientManager.UserClient.GetUserProfileAsync(p.Username);

                                playerInfos.Add(new PlayerLobbyInfo
                                {
                                    Username = profile.Username,
                                    AvatarUri = LoadAvatar(profile.AvatarId)
                                });
                            }
                            catch
                            {
                                playerInfos.Add(new PlayerLobbyInfo
                                {
                                    Username = p.Username,
                                    AvatarUri = LoadAvatar("avatar_aaa_default")
                                });
                            }
                        }

                        PlayersList.ItemsSource = playerInfos;

                        isOwner = players.Any(p => p.OwnerUsername == SessionManager.CurrentUsername);  
                        btnStart.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                        MessageBox.Show(string.Format(Lang.ExceptionTextErrorLoadingPlayers, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error));
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

            private BitmapImage LoadAvatar(string avatarId)
            {
                string path = $"pack://application:,,,/TrucoClient;component/Resources/Avatars/{avatarId}.png";
                return new BitmapImage(new Uri(path, UriKind.Absolute));
            }

            private void InitializeChat()
            {
                AddChatMessage(string.Empty, string.Format(Lang.LobbyTextJoinedRoom, matchCode));
            }
        }
    }
