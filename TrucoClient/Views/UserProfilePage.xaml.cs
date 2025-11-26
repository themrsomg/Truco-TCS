using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.ServiceModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class UserProfilePage : Page
    {
        private const string MESSAGE_ERROR = "Error";
        private const string FACEBOOK_BASE_URL = "https://www.facebook.com/";
        private const string X_BASE_URL = "https://x.com/";
        private const string INSTAGRAM_BASE_URL = "https://www.instagram.com/";
        private const int MAX_CHANGES = 2;
        private const int MIN_USERNAME_LENGTH = 4;
        private const int MAX_USERNAME_LENGTH = 20;

        private UserProfileData localEditingData;

        private string originalUsername;
        private string originalFacebook;
        private string originalX;
        private string originalInstagram;

        public UserProfilePage()
        {
            InitializeComponent();
            originalUsername = string.Empty;
            LoadUserProfile();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void ClickSave(object sender, RoutedEventArgs e)
        {
            if (localEditingData == null)
            {
                return;
            }

            string newUsername = txtUsername.Text.Trim();

            string newFacebook = ExtractHandle(txtFacebookLink.Text, FACEBOOK_BASE_URL);
            string newX = ExtractHandle(txtXLink.Text, X_BASE_URL);
            string newInstagram = ExtractHandle(txtInstagramLink.Text, INSTAGRAM_BASE_URL);

            txtFacebookLink.Text = newFacebook;
            txtXLink.Text = newX;
            txtInstagramLink.Text = newInstagram;

            if (!ValidateUsernameChange(newUsername))
            {
                return;
            }

            if (!HasChangesToSave(newUsername, newFacebook, newX, newInstagram))
            {
                return;
            }

            if (!ConfirmSaveChanges())
            {
                return;
            }

            await SaveUserProfileAsync(newUsername, newFacebook, newX, newInstagram, sender as Button);
            UpdateSocialMediaLinks();
        }

        private void ClickChangeAvatar(object sender, RoutedEventArgs e)
        {
            if (localEditingData == null)
            {
                return;
            }

            var avatarPage = new AvatarSelectionPage(AvatarHelper.availableAvatars.ToList(), localEditingData.AvatarId);
            avatarPage.AvatarSelected += AvatarSelectedHandler;
            NavigationService.Navigate(avatarPage);
        }

        private void ClickChangePassword(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ChangePasswordPage());
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox == txtUsername)
            {
                if (!FieldValidator.IsRequired(textBox.Text))
                {
                    ErrorDisplayService.ShowError(textBox, blckUsernameError, Lang.GlobalTextRequieredField);
                }
                else
                {
                    ValidateUsernameInput(textBox);
                }
            }

            UpdateSocialMediaLinks();
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            ClearSpecificError(textBox);

            if (textBox == txtUsername)
            {
                ValidateUsernameInput(textBox);
            }
            UpdateSocialMediaLinks();
        }

        private void UsernamePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !UsernameValidator.IsValidFormat(e.Text);
        }

        private void OpenSocialLink(string handle, string baseUrl)
        {
            try
            {
                string cleanHandle = ExtractHandle(handle, baseUrl);

                if (!string.IsNullOrWhiteSpace(cleanHandle))
                {
                    string finalUrl = baseUrl + cleanHandle;
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = finalUrl,
                        UseShellExecute = true
                    });
                }
            }
            catch (Win32Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FormatException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClickHyperlinkFacebook(object sender, RoutedEventArgs e)
        {
            OpenSocialLink(txtFacebookLink.Text, FACEBOOK_BASE_URL);
        }

        private void ClickHyperlinkX(object sender, RoutedEventArgs e)
        {
            OpenSocialLink(txtXLink.Text, X_BASE_URL);
        }

        private void ClickHyperlinkInstagram(object sender, RoutedEventArgs e)
        {
            OpenSocialLink(txtInstagramLink.Text, INSTAGRAM_BASE_URL);
        }

        private async void AvatarSelectedHandler(object sender, string newAvatarId)
        {
            AvatarSelectionPage avatarPage = sender as AvatarSelectionPage;
            if (avatarPage != null)
            {
                avatarPage.AvatarSelected -= AvatarSelectedHandler;
            }

            if (newAvatarId == localEditingData.AvatarId)
            {
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                var userClient = ClientManager.UserClient;
                bool success = await userClient.UpdateUserAvatarAsync(SessionManager.CurrentUsername, newAvatarId);

                if (success)
                {
                    localEditingData.AvatarId = newAvatarId;
                    SessionManager.CurrentUserData.AvatarId = newAvatarId;

                    AvatarHelper.LoadAvatarImage(imgAvatar, newAvatarId);
                    CustomMessageBox.Show(Lang.UserProfileTextAvatarSuccess, Lang.GlobalTextSuccess,
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    CustomMessageBox.Show(Lang.UserProfileTextAvatarError, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (EndpointNotFoundException)
            {
                ShowConnectionError();
            }
            catch (Exception)
            {
                ShowGeneralError();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void LoadUserProfile()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                if (SessionManager.CurrentUserData == null)
                {
                    return;
                }

                var sessionData = SessionManager.CurrentUserData;

                localEditingData = new UserProfileData
                {
                    Username = sessionData.Username,
                    Email = sessionData.Email,
                    AvatarId = sessionData.AvatarId,
                    NameChangeCount = sessionData.NameChangeCount,
                    FacebookHandle = sessionData.FacebookHandle,
                    XHandle = sessionData.XHandle,
                    InstagramHandle = sessionData.InstagramHandle
                };

                originalUsername = localEditingData.Username ?? string.Empty;
                originalFacebook = localEditingData.FacebookHandle ?? string.Empty;
                originalX = localEditingData.XHandle ?? string.Empty;
                originalInstagram = localEditingData.InstagramHandle ?? string.Empty;

                this.DataContext = localEditingData;

                AvatarHelper.LoadAvatarImage(imgAvatar, localEditingData.AvatarId);

                txtUsername.Text = localEditingData.Username;
                txtEmail.Text = localEditingData.Email;
                txtFacebookLink.Text = localEditingData.FacebookHandle;
                txtXLink.Text = localEditingData.XHandle;
                txtInstagramLink.Text = localEditingData.InstagramHandle;

                UpdateUsernameWarning(localEditingData.NameChangeCount);
                UpdateSocialMediaLinks();
            }
            catch (EndpointNotFoundException)
            {
                ShowConnectionError();
            }
            catch (Exception)
            {
                ShowGeneralError();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async Task SaveUserProfileAsync(string newUsername, string newFacebook, string newX, string newInstagram, Button saveButton)
        {
            saveButton.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            string oldUsername = originalUsername;
            int oldChangeCount = localEditingData.NameChangeCount;

            try
            {
                var userClient = ClientManager.UserClient;

                UpdateLocalData(newUsername, newFacebook, newX, newInstagram);

                if (!string.Equals(newUsername, originalUsername, StringComparison.Ordinal)
                    && await UsernameExistsAsync(newUsername, userClient))
                {
                    localEditingData.Username = oldUsername;
                    txtUsername.Text = oldUsername;
                    return;
                }

                bool success = await userClient.SaveUserProfileAsync(localEditingData);

                if (success)
                {
                    HandleSuccessfulSave(newUsername, newFacebook, newX, newInstagram);
                }
                else
                {
                    HandleFailedSave(oldUsername, oldChangeCount, originalFacebook, originalX, originalInstagram);
                }
            }
            catch (EndpointNotFoundException)
            {
                ShowConnectionError();
                HandleFailedSave(oldUsername, oldChangeCount, originalFacebook, originalX, originalInstagram);
            }
            catch (Exception)
            {
                ShowGeneralError();
                HandleFailedSave(oldUsername, oldChangeCount, originalFacebook, originalX, originalInstagram);
            }
            finally
            {
                saveButton.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        private bool ValidateUsernameChange(string newUsername)
        {
            ClearAllErrors();
            bool isValid = true;
            bool usernameChanged = !string.Equals(newUsername, originalUsername, StringComparison.Ordinal);

            if (!FieldValidator.IsRequired(newUsername))
            {
                ErrorDisplayService.ShowError(txtUsername, blckUsernameError, Lang.GlobalTextRequieredField);
                isValid = false;
            }
            else if (usernameChanged)
            {
                if (!UsernameValidator.IsValidFormat(newUsername))
                {
                    ErrorDisplayService.ShowError(txtUsername, blckUsernameError, Lang.GlobalTextInvalidUsername);
                    isValid = false;
                }

                if (!UsernameValidator.ValidateLength(newUsername, MIN_USERNAME_LENGTH, MAX_USERNAME_LENGTH))
                {
                    string message = newUsername.Length < MIN_USERNAME_LENGTH ? Lang.DialogTextShortUsername : Lang.DialogTextLongUsername;
                    ErrorDisplayService.ShowError(txtUsername, blckUsernameError, message);
                    isValid = false;
                }

                if (localEditingData.NameChangeCount >= MAX_CHANGES)
                {
                    ErrorDisplayService.ShowError(txtUsername, blckUsernameError, Lang.UserProfileTextChangesError);
                    txtUsername.Text = originalUsername;
                    isValid = false;
                }
            }

            return isValid;
        }

        private void ValidateUsernameInput(TextBox textBox)
        {
            string text = textBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (!UsernameValidator.IsValidFormat(text))
            {
                ErrorDisplayService.ShowError(textBox, blckUsernameError, Lang.GlobalTextInvalidUsername);
            }
            else if (text.Length < MIN_USERNAME_LENGTH)
            {
                ErrorDisplayService.ShowError(textBox, blckUsernameError, Lang.DialogTextShortUsername);
            }
            else if (text.Length > MAX_USERNAME_LENGTH)
            {
                ErrorDisplayService.ShowError(textBox, blckUsernameError, Lang.DialogTextLongUsername);
            }
        }

        private async Task<bool> UsernameExistsAsync(string username, TrucoUserServiceClient userClient)
        {
            bool exists = await Task.Run(() => userClient.UsernameExists(username));

            if (exists)
            {
                ErrorDisplayService.ShowError(txtUsername, blckUsernameError, Lang.GlobalTextUsernameAlreadyInUse);
            }

            return exists;
        }

        private void ClearSpecificError(Control field)
        {
            TextBlock errorBlock = null;

            if (field == txtUsername)
            {
                errorBlock = blckUsernameError;
            }
            else if (field == txtFacebookLink)
            {
                errorBlock = blckFacebookError;
            }
            else if (field == txtXLink)
            {
                errorBlock = blckXError;
            }
            else if (field == txtInstagramLink)
            {
                errorBlock = blckInstagramError;
            }
            
            ErrorDisplayService.ClearError(field, errorBlock);
        }

        private void ClearAllErrors()
        {
            ErrorDisplayService.ClearError(txtUsername, blckUsernameError);
            ErrorDisplayService.ClearError(txtFacebookLink, blckFacebookError);
            ErrorDisplayService.ClearError(txtXLink, blckXError);
            ErrorDisplayService.ClearError(txtInstagramLink, blckInstagramError);
        }

        private string Normalize(string input)
        {
            return input?.Trim() ?? string.Empty;
        }

        private bool HasChangesToSave(string newUsername, string newFacebook, string newX, string newInstagram)
        {
            string currentFb = Normalize(originalFacebook);
            string currentX = Normalize(originalX);
            string currentIg = Normalize(originalInstagram);
            string currentUser = Normalize(originalUsername);

            string inputFb = Normalize(newFacebook);
            string inputX = Normalize(newX);
            string inputIg = Normalize(newInstagram);
            string inputUser = Normalize(newUsername);

            bool usernameChanged = !string.Equals(inputUser, currentUser, StringComparison.Ordinal);
            bool facebookChanged = !string.Equals(inputFb, currentFb, StringComparison.Ordinal);
            bool xChanged = !string.Equals(inputX, currentX, StringComparison.Ordinal);
            bool instagramChanged = !string.Equals(inputIg, currentIg, StringComparison.Ordinal);

            bool changed = usernameChanged || facebookChanged || xChanged || instagramChanged;

            if (!changed)
            {
                CustomMessageBox.Show(Lang.UserProfileTextSaveNoChanges, Lang.UserProfileTextNoChanges,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return changed;
        }

        private static bool ConfirmSaveChanges()
        {
            bool? confirm = CustomMessageBox.Show(Lang.UserProfileTextConfirmChanges,
                Lang.GlobalTextConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);

            return confirm == true;
        }

        private void UpdateLocalData(string newUsername, string fb, string x, string ig)
        {
            localEditingData.Username = newUsername;
            localEditingData.FacebookHandle = fb;
            localEditingData.XHandle = x;
            localEditingData.InstagramHandle = ig;
        }

        private void HandleSuccessfulSave(string newUsername, string newFacebook, string newX, string newInstagram)
        {
            if (!string.Equals(newUsername, originalUsername, StringComparison.Ordinal))
            {
                localEditingData.NameChangeCount++;
            }

            originalUsername = newUsername;
            originalFacebook = newFacebook;
            originalX = newX;
            originalInstagram = newInstagram;

            SessionManager.CurrentUserData.Username = newUsername;
            SessionManager.CurrentUserData.NameChangeCount = localEditingData.NameChangeCount;
            SessionManager.CurrentUserData.FacebookHandle = newFacebook;
            SessionManager.CurrentUserData.XHandle = newX;
            SessionManager.CurrentUserData.InstagramHandle = newInstagram;

            SessionManager.CurrentUsername = newUsername;

            LoadUserProfile();

            CustomMessageBox.Show(Lang.UserProfileTextSuccess, Lang.GlobalTextSuccess,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleFailedSave(string oldUsername, int oldChangeCount, string oldFacebook, string oldX, string oldInstagram)
        {
            localEditingData.Username = oldUsername;
            localEditingData.NameChangeCount = oldChangeCount;
            localEditingData.FacebookHandle = oldFacebook;
            localEditingData.XHandle = oldX;
            localEditingData.InstagramHandle = oldInstagram;

            txtUsername.Text = oldUsername;
            txtFacebookLink.Text = oldFacebook;
            txtXLink.Text = oldX;
            txtInstagramLink.Text = oldInstagram;

            CustomMessageBox.Show(Lang.UserProfileTextErrorSaving, MESSAGE_ERROR,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void UpdateUsernameWarning(int count)
        {
            int remaining = MAX_CHANGES - count;
            txtUsernameWarning.Text = string.Format(Lang.UserProfileTextUsernameChangesWarning, remaining, MAX_CHANGES);
            txtUsernameWarning.Visibility = Visibility.Visible;

            if (remaining <= 0)
            {
                txtUsernameWarning.Text = Lang.UserProfileTextChangesError;
                txtUsernameWarning.Foreground = new SolidColorBrush(Colors.Red);
                txtUsername.IsReadOnly = true;
            }
            else if (remaining == 1)
            {
                txtUsernameWarning.Foreground = new SolidColorBrush(Colors.Orange);
                txtUsername.IsReadOnly = false;
            }
            else
            {
                txtUsernameWarning.Foreground = new SolidColorBrush(Colors.White);
                txtUsername.IsReadOnly = false;
            }
        }

        private void UpdateSocialMediaLinks()
        {
            linkFacebookContainer.Visibility = string.IsNullOrWhiteSpace(txtFacebookLink.Text) ? Visibility.Collapsed : Visibility.Visible;
            linkXContainer.Visibility = string.IsNullOrWhiteSpace(txtXLink.Text) ? Visibility.Collapsed : Visibility.Visible;
            linkInstagramContainer.Visibility = string.IsNullOrWhiteSpace(txtInstagramLink.Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private static void ShowConnectionError()
        {
            CustomMessageBox.Show(Lang.ExceptionTextConnectionError,
                Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void ShowGeneralError()
        {
            CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private string ExtractHandle(string input, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            string cleaned = input.Trim();

            if (cleaned.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring(baseUrl.Length);
            }
            return cleaned;
        }
    }
}