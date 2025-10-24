using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Data;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using TrucoPrueba1.TrucoServer;
using TrucoPrueba1.Properties.Langs;

namespace TrucoPrueba1
{
    public class FriendDisplayData
    {
        public string Username { get; set; }
        public string AvatarId { get; set; }

        public string AvatarImagePath
        {
            get
            {
                string id = string.IsNullOrWhiteSpace(AvatarId) ? "avatar_aaa_default" : AvatarId;
                string path = $"pack://application:,,,/TrucoPrueba1;component/Resources/Avatars/{id}.png";
                try
                {
                    var test = new BitmapImage(new Uri(path));
                    return path;
                }
                catch
                {
                    return "pack://application:,,,/TrucoPrueba1;component/Resources/Avatars/avatar_aaa_default.png";
                }
            }
        }
    }

    public partial class FriendsPage : Page
    {
        public ObservableCollection<FriendDisplayData> FriendsList { get; set; } = new ObservableCollection<FriendDisplayData>();
        public ObservableCollection<FriendDisplayData> PendingList { get; set; } = new ObservableCollection<FriendDisplayData>();
        public FriendDisplayData SelectedFriend { get; set; }
        public FriendDisplayData SelectedPending { get; set; }

        public FriendsPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
            this.DataContext = this;
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            FriendsList.Clear();
            PendingList.Clear();

            string currentUsername = SessionManager.CurrentUsername;

            if (string.IsNullOrEmpty(currentUsername))
            {
                return;
            }

            using (var friendClient = new TrucoFriendServiceClient(new InstanceContext(new TrucoCallbackHandler())))
            {
                try
                {
                    var friends = await friendClient.GetFriendsAsync(currentUsername);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var friend in friends)
                        {
                            FriendsList.Add(new FriendDisplayData { Username = friend.Username, AvatarId = friend.AvatarId });
                        }
                    });

                    var pending = await friendClient.GetPendingFriendRequestsAsync(currentUsername);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var req in pending)
                        {
                            PendingList.Add(new FriendDisplayData { Username = req.Username, AvatarId = req.AvatarId });
                        }
                    });

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ClickAddFriend(object sender, RoutedEventArgs e)
        {
            string targetUsername = txtSearch.Text.Trim();
            string currentUsername = SessionManager.CurrentUsername;

            if (string.IsNullOrEmpty(targetUsername))
            {
                MessageBox.Show(Lang.FriendsTextRequestInvalidUser, Lang.GlobalTextWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (targetUsername.Equals(currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(Lang.FriendsTextRequestSelf, Lang.GlobalTextWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var friendClient = new TrucoFriendServiceClient(new InstanceContext(new TrucoCallbackHandler())))
            {
                try
                {
                    bool success = await friendClient.SendFriendRequestAsync(currentUsername, targetUsername);

                    if (success)
                    {
                        MessageBox.Show(string.Format(Lang.FriendsTextRequestSuccess, targetUsername), Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(Lang.FriendsTextRequestError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ClickAcceptRequest(object sender, RoutedEventArgs e)
        {
            string requesterUsername = (sender as Button)?.Tag?.ToString();
            if (string.IsNullOrEmpty(requesterUsername))
            {
                return;
            }

            string currentUsername = SessionManager.CurrentUsername;

            using (var friendClient = new TrucoFriendServiceClient(new InstanceContext(new TrucoCallbackHandler())))
            {
                try
                {
                    bool success = await friendClient.AcceptFriendRequestAsync(requesterUsername, currentUsername);

                    if (success)
                    {
                        MessageBox.Show(string.Format(Lang.FriendsTextRequestAccepted, requesterUsername), Lang.FriendsTextRequestAcceptedTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show(Lang.FriendsTextRequestAcceptedError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ClickRejectRequest(object sender, RoutedEventArgs e)
        {
            string targetUsername = (sender as Button)?.Tag?.ToString();
            if (string.IsNullOrEmpty(targetUsername))
            {
                return;
            }

            string currentUsername = SessionManager.CurrentUsername;

            using (var friendClient = new TrucoFriendServiceClient(new InstanceContext(new TrucoCallbackHandler())))
            {
                try
                {
                    bool success = await friendClient.RemoveFriendOrRequestAsync(targetUsername, currentUsername);

                    if (success)
                    {
                        MessageBox.Show(string.Format(Lang.FriendsTextRequestRejected, targetUsername), Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show(Lang.FriendsTextRequestRejectedError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }
        private void txtSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlaceholderText != null)
            {
                PlaceholderText.Visibility = string.IsNullOrEmpty(txtSearch.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }
}
