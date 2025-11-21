    using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.TrucoServer;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using System.Linq;

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

        private UserProfileData currentUserData;
        private string originalUsername;

        public UserProfilePage()
        {
            InitializeComponent();
            originalUsername = string.Empty;
            LoadUserProfile();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void ClickSave(object sender, RoutedEventArgs e)
        {
            if (currentUserData == null)
            {
                return;
            }

            string newUsername = txtUsername.Text.Trim();
            string newFacebook = txtFacebookLink.Text.Trim();
            string newX = txtXLink.Text.Trim();
            string newInstagram = txtInstagramLink.Text.Trim();

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
        }

        private void ClickChangeAvatar(object sender, RoutedEventArgs e)
        {
            if (currentUserData == null)
            {
                return;
            }

            var avatarPage = new AvatarSelectionPage(AvatarHelper.availableAvatars.ToList(), currentUserData.AvatarId);
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
        }

        private void UsernamePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !UsernameValidator.IsValidFormat(e.Text);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                string handle = string.Empty;
                string baseUrl = string.Empty;

                if (sender == linkFacebook)
                {
                    handle = txtFacebookLink.Text.Trim();
                    baseUrl = FACEBOOK_BASE_URL;
                }
                else if (sender == linkX)
                {
                    handle = txtXLink.Text.Trim();
                    baseUrl = X_BASE_URL;
                }
                else if (sender == linkInstagram)
                {
                    handle = txtInstagramLink.Text.Trim();
                    baseUrl = INSTAGRAM_BASE_URL;
                }

                if (!string.IsNullOrWhiteSpace(handle))
                {
                    string finalUrl = baseUrl + handle;
                    if (Uri.IsWellFormedUriString(finalUrl, UriKind.Absolute))
                    {
                        Process.Start(new ProcessStartInfo(finalUrl)
                        {
                            UseShellExecute = true
                        });
                    }
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void AvatarSelectedHandler(object sender, string newAvatarId)
        {
            AvatarSelectionPage avatarPage = sender as AvatarSelectionPage;
            if (avatarPage != null)
            {
                avatarPage.AvatarSelected -= AvatarSelectedHandler;
            }

            if (newAvatarId == currentUserData.AvatarId)
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
                    currentUserData.AvatarId = newAvatarId;

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
            catch (System.ServiceModel.EndpointNotFoundException)
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

                currentUserData = SessionManager.CurrentUserData;
                originalUsername = currentUserData.Username ?? string.Empty;

                this.DataContext = currentUserData;

                AvatarHelper.LoadAvatarImage(imgAvatar, currentUserData.AvatarId);

                txtUsername.Text = currentUserData.Username;
                txtEmail.Text = currentUserData.Email;
                txtFacebookLink.Text = currentUserData.FacebookHandle;
                txtXLink.Text = currentUserData.XHandle;
                txtInstagramLink.Text = currentUserData.InstagramHandle;

                UpdateUsernameWarning(currentUserData.NameChangeCount);
                UpdateSocialMediaLinks();
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

            string oldUsername = currentUserData.Username;
            int oldChangeCount = currentUserData.NameChangeCount;

            string oldFacebook = currentUserData.FacebookHandle;
            string oldX = currentUserData.XHandle;
            string oldInstagram = currentUserData.InstagramHandle;


            try
            {
                var userClient = ClientManager.UserClient;
                UpdateCurrentUserData(newUsername, newFacebook, newX, newInstagram);

                if (!string.Equals(newUsername, originalUsername, StringComparison.Ordinal) 
                    && await UsernameExistsAsync(newUsername, userClient))
                {
                    currentUserData.Username = oldUsername;
                    txtUsername.Text = oldUsername;
                    return;
                }

                bool success = await userClient.SaveUserProfileAsync(currentUserData);

                if (success)
                {
                    HandleSuccessfulSave(newUsername);
                }
                else
                {
                    HandleFailedSave(oldUsername, oldChangeCount, oldFacebook, oldX, oldInstagram);
                }
            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                ShowConnectionError();
            }
            catch (Exception)
            {
                ShowGeneralError();
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

                if (currentUserData.NameChangeCount >= MAX_CHANGES)
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

            /*
             *else if (field == txtFacebookLink) { errorBlock = blckFacebookError; }
             *else if (field == txtXLink) { errorBlock = blckXError; }
             *else if (field == txtInstagramLink) { errorBlock = blckInstagramError; }
             */

            ErrorDisplayService.ClearError(field, errorBlock);
        }

        private void ClearAllErrors()
        {
            ErrorDisplayService.ClearError(txtUsername, blckUsernameError);
            // ErrorDisplayService.ClearError(txtFacebookLink, blckFacebookError); 
            // ErrorDisplayService.ClearError(txtXLink, blckXError); 
            // ErrorDisplayService.ClearError(txtInstagramLink, blckInstagramError); 
        }

        private bool HasChangesToSave(string newUsername, string newFacebook, string newX, string newInstagram)
        {
            bool usernameChanged = !string.Equals(newUsername, originalUsername, StringComparison.Ordinal);
            bool facebookChanged = !string.Equals(newFacebook, currentUserData.FacebookHandle, StringComparison.Ordinal);
            bool xChanged = !string.Equals(newX, currentUserData.XHandle, StringComparison.Ordinal);
            bool instagramChanged = !string.Equals(newInstagram, currentUserData.InstagramHandle, StringComparison.Ordinal);

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

        private void UpdateCurrentUserData(string newUsername, string fb, string x, string ig)
        {
            if (!string.Equals(newUsername, currentUserData.Username, StringComparison.Ordinal))
            {
                currentUserData.Username = newUsername;
            }

            if (!string.Equals(fb, currentUserData.FacebookHandle, StringComparison.Ordinal))
            {
                currentUserData.FacebookHandle = fb;
            }

            if (!string.Equals(x, currentUserData.XHandle, StringComparison.Ordinal))
            {
                currentUserData.XHandle = x;
            }

            if (!string.Equals(ig, currentUserData.InstagramHandle, StringComparison.Ordinal))
            {
                currentUserData.InstagramHandle = ig;
            }
        }

        private void HandleSuccessfulSave(string newUsername)
        {
            if (!string.Equals(newUsername, originalUsername, StringComparison.Ordinal))
            {
                currentUserData.NameChangeCount++;
            }

            LoadUserProfile();

            SessionManager.CurrentUserData = currentUserData;
            SessionManager.CurrentUsername = currentUserData.Username;
            originalUsername = currentUserData.Username;

            CustomMessageBox.Show(Lang.UserProfileTextSuccess, Lang.GlobalTextSuccess, 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleFailedSave(string oldUsername, int oldChangeCount, string oldFacebook, string oldX, string oldInstagram)
        {
            currentUserData.Username = oldUsername;
            currentUserData.NameChangeCount = oldChangeCount;
            currentUserData.FacebookHandle = oldFacebook;
            currentUserData.XHandle = oldX;
            currentUserData.InstagramHandle = oldInstagram;

            txtUsername.Text = oldUsername;

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
    }
}