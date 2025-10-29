using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient
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
                string path = $"pack://application:,,,/TrucoClient;component/Resources/Avatars/{id}.png";
                try
                {
                    var test = new BitmapImage(new Uri(path));
                    return path;
                }
                catch (Exception ex)
                {
                    return "pack://application:,,,/TrucoClient;component/Resources/Avatars/avatar_aaa_default.png";
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
                catch (System.ServiceModel.EndpointNotFoundException ex)
                {
                    MessageBox.Show($"No se pudo conectar al servidor: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ClickAddFriend(object sender, RoutedEventArgs e)
        {
            string targetUsername = txtSearch.Text.Trim();
            string currentUsername = SessionManager.CurrentUsername;

            if (!FieldValidation(targetUsername, currentUsername))
            {
                return;
            }
            
            try
            {
                var friendClient = ClientManager.FriendClient;

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

        private bool FieldValidation(string targetUsername, string currentUsername)
        {
            ClearError(txtSearch);
            bool isValid = true;

            if (string.IsNullOrEmpty(targetUsername))
            {
                ShowError(txtSearch, Lang.GlobalTextRequieredField);
                isValid = false;
            }

            if (!isValid)
            {
                return false;
            }

            if (targetUsername.Length < 4)
            {
                ShowError(txtSearch, Lang.DialogTextShortUsername);
                isValid = false;
            }
            else if (targetUsername.Length > 20)
            {
                ShowError(txtSearch, Lang.DialogTextLongUsername);
                isValid = false;
            }

            if (targetUsername.Equals(currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                ShowError(txtSearch, Lang.FriendsTextRequestSelf);
                isValid = false;
            }

            return isValid;
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

        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (blckPlaceholder != null)
            {
                blckPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            ClearError(txtSearch);

            string text = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (text.Length < 4)
            {
                ShowError(txtSearch, Lang.DialogTextShortUsername);
            }
            else if (text.Length > 20)
            {
                ShowError(txtSearch, Lang.DialogTextLongUsername);
            }
        }

        private void ShowError(Control field, string errorMessage)
        {
            TextBlock errorBlock = blckFriendError;

            if (errorBlock != null)
            {
                errorBlock.Text = errorMessage;
            }

            field.BorderBrush = new SolidColorBrush(Colors.Red);
        }

        private void ClearError(Control field)
        {
            TextBlock errorBlock = blckFriendError;

            if (errorBlock != null)
            {
                errorBlock.Text = string.Empty;
            }

            field.ClearValue(Border.BorderBrushProperty);
        }
    }
}
