using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Paths;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public class FriendDisplayData
    {
        private const string MESSAGE_ERROR = "Error";

        public string Username { get; set; }
        public string AvatarId { get; set; }

        public string AvatarImagePath
        {
            get
            {
                string id = string.IsNullOrWhiteSpace(AvatarId) ? "avatar_aaa_default" : AvatarId;
                string correctedPath = $"/Resources/Avatars/{id}.png";

                try
                {
                    _ = new BitmapImage(new Uri(correctedPath, UriKind.Relative));
                    return correctedPath;
                }
                catch (UriFormatException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    return ResourcePaths.DEFAULT_AVATAR_PATH;
                }
                catch (Exception)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    return ResourcePaths.DEFAULT_AVATAR_PATH;
                }
            }
        }
    }

    public partial class FriendsPage : Page
    {
        private const int MIN_USERNAME_LENGTH = 4;
        private const int MAX_USERNAME_LENGTH = 20;
        private const string MESSAGE_ERROR = "Error";

        private static readonly Regex usernameInputRegex = new Regex(@"^[a-zA-Z0-9_]*$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        public ObservableCollection<FriendDisplayData> FriendsList { get; set; } = new ObservableCollection<FriendDisplayData>();
        public ObservableCollection<FriendDisplayData> PendingList { get; set; } = new ObservableCollection<FriendDisplayData>();
        public FriendDisplayData SelectedFriend { get; set; }
        public FriendDisplayData SelectedPending { get; set; }

        public FriendsPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
            this.DataContext = this;

            InitializeValidation();

            _ = LoadDataAsync();
        }

        private void InitializeValidation()
        {
            InputRestriction.AttachRegexValidation(txtSearch, usernameInputRegex);
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

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var friend in friends)
                        {
                            FriendsList.Add(new FriendDisplayData { Username = friend.Username, AvatarId = friend.AvatarId });
                        }
                    });

                    var pending = await friendClient.GetPendingFriendRequestsAsync(currentUsername);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var req in pending)
                        {
                            PendingList.Add(new FriendDisplayData { Username = req.Username, AvatarId = req.AvatarId });
                        }
                    });

                }
                catch (EndpointNotFoundException ex)
                {
                    ClientException.HandleError(ex, nameof(LoadDataAsync));
                    CustomMessageBox.Show(Lang.ExceptionTextConnectionError,
                        Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
                    CustomMessageBox.Show(string.Format(Lang.FriendsTextRequestSuccess, targetUsername),
                        Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);

                    txtSearch.Text = string.Empty;
                    ErrorDisplayService.ClearError(txtSearch, blckFriendError);
                }
                else
                {
                    CustomMessageBox.Show(Lang.FriendsTextRequestError, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (EndpointNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(ClickAddFriend));
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError,
                    Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private bool FieldValidation(string targetUsername, string currentUsername)
        {
            ErrorDisplayService.ClearError(txtSearch, blckFriendError);

            if (!FieldValidator.IsRequired(targetUsername))
            {
                ErrorDisplayService.ShowError(txtSearch, blckFriendError, Lang.GlobalTextRequieredField);
                return false;
            }

            if (!FieldValidator.IsLengthInRange(targetUsername, MIN_USERNAME_LENGTH, MAX_USERNAME_LENGTH))
            {
                string errorMsg = targetUsername.Length < MIN_USERNAME_LENGTH
                    ? Lang.DialogTextShortUsername
                    : Lang.DialogTextLongUsername;

                ErrorDisplayService.ShowError(txtSearch, blckFriendError, errorMsg);
                return false;
            }

            if (!UsernameValidator.IsValidFormat(targetUsername))
            {
                ErrorDisplayService.ShowError(txtSearch, blckFriendError, Lang.GlobalTextInvalidUsername);
                return false;
            }

            if (targetUsername.Equals(currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                ErrorDisplayService.ShowError(txtSearch, blckFriendError, Lang.FriendsTextRequestSelf);
                return false;
            }

            return true;
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
                        CustomMessageBox.Show(string.Format(Lang.FriendsTextRequestAccepted, requesterUsername),
                            Lang.FriendsTextRequestAcceptedTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        CustomMessageBox.Show(Lang.FriendsTextRequestAcceptedError, MESSAGE_ERROR,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (EndpointNotFoundException ex)
                {
                    ClientException.HandleError(ex, nameof(ClickAcceptRequest));
                    CustomMessageBox.Show(Lang.ExceptionTextConnectionError,
                        Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception)
                {
                    CustomMessageBox.Show(Lang.FriendsTextRequestAcceptedError, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Error);
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
                        CustomMessageBox.Show(string.Format(Lang.FriendsTextRequestRejected, targetUsername),
                            Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        CustomMessageBox.Show(Lang.FriendsTextRequestRejectedError, MESSAGE_ERROR,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (EndpointNotFoundException ex)
                {
                    ClientException.HandleError(ex, nameof(ClickRejectRequest));
                    CustomMessageBox.Show(Lang.ExceptionTextConnectionError,
                        Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception)
                {
                    CustomMessageBox.Show(Lang.FriendsTextRequestAcceptedError, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Error);
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

            ErrorDisplayService.ClearError(txtSearch, blckFriendError);

            string text = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (!FieldValidator.IsLengthInRange(text, MIN_USERNAME_LENGTH, MAX_USERNAME_LENGTH))
            {
                string errorMsg = text.Length < MIN_USERNAME_LENGTH
                   ? Lang.DialogTextShortUsername
                   : Lang.DialogTextLongUsername;

                ErrorDisplayService.ShowError(txtSearch, blckFriendError, errorMsg);
            }
        }

        private void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender == txtSearch)
            {
                ClickAddFriend(btnAdd, null);
                e.Handled = true;
            }
        }
    }
}