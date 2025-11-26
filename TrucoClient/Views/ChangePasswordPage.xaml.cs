using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ServiceModel;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;

namespace TrucoClient.Views
{
    public partial class ChangePasswordPage : Page
    {
        private const int NEW_PASSWORD_MIN_LENGTH = 12;
        private const int PASSWORD_MAX_LENGTH = 50;
        private const int CURRENT_PASSWORD_MIN_LENGTH = 8;
        private const string MESSAGE_ERROR = "Error";

        private string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        public ChangePasswordPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickSave(object sender, RoutedEventArgs e)
        {
            string currentPassword = txtCurrentPassword.Password.Trim();
            string newPassword = txtPassword.Password.Trim();
            string confirmPassword = txtPasswordConfirm.Password.Trim();

            ClearAllErrors();
            
            if (!FieldsValidation(currentPassword, newPassword, confirmPassword))
            {
                return;
            }

            btnSave.IsEnabled = false;

            try
            {
                var userClient = ClientManager.UserClient;
                string email = SessionManager.CurrentUserData.Email;

                bool changed = userClient.PasswordChange(email, newPassword, languageCode);

                if (changed)
                {
                    CustomMessageBox.Show(Lang.DialogTextPasswordChangedSuccess, Lang.GlobalTextSuccess, 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.NavigationService.Navigate(new LogInPage());
                }
                else
                {
                    CustomMessageBox.Show(Lang.DialogTextPasswordChangeError, MESSAGE_ERROR, 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (EndpointNotFoundException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError, 
                    Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (this.IsLoaded)
                {
                    btnSave.IsEnabled = true;
                }
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            bool shouldNavigate = false;

            if (HasUnsavedFields())
            {
                bool? result = CustomMessageBox.Show(Lang.DialogTextConfirmationNewUser, Lang.GlobalTextConfirmation, 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == true)
                {
                    shouldNavigate = true;
                }
            }
            else
            {
                shouldNavigate = true;
            }

            if (shouldNavigate)
            {
                this.NavigationService.Navigate(new UserProfilePage());
            }
        }

        private void ClickForgotPassword(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ForgotPasswordStepOnePage());
        }

        private bool HasUnsavedFields()
        {
            bool allEmpty = string.IsNullOrEmpty(txtCurrentPassword.Password.Trim()) &&
                            string.IsNullOrEmpty(txtPassword.Password.Trim()) &&
                            string.IsNullOrEmpty(txtPasswordConfirm.Password.Trim());

            return !allEmpty;
        }

        private bool FieldsValidation(string currentPassword, string newPassword, string confirmPassword)
        {
            bool areValid = true;

            if (!FieldValidator.IsRequired(currentPassword))
            {
                ErrorDisplayService.ShowError(txtCurrentPassword, blckCurrentError, Lang.GlobalTextRequieredField);
                areValid = false;
            }
            else if (!FieldValidator.IsLengthInRange(currentPassword, CURRENT_PASSWORD_MIN_LENGTH, PASSWORD_MAX_LENGTH))
            {
                ErrorDisplayService.ShowError(txtCurrentPassword, blckCurrentError, Lang.DialogTextShortPassword);
                areValid = false;
            }

            if (!FieldValidator.IsRequired(newPassword))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.GlobalTextRequieredField);
                areValid = false;
            }
            else if (!PasswordValidator.ValidateLength(newPassword, NEW_PASSWORD_MIN_LENGTH, PASSWORD_MAX_LENGTH))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextShortPassword);
                areValid = false;
            }
            else if (!PasswordValidator.IsComplex(newPassword))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.GlobalTextPasswordNoComplex);
                areValid = false;
            }

            if (!FieldValidator.IsRequired(confirmPassword))
            {
                ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, Lang.GlobalTextRequieredField);
                areValid = false;
            }
            else if (!PasswordValidator.AreMatching(newPassword, confirmPassword))
            {
                ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, Lang.DialogTextPasswordsDontMatch);
                areValid = false;
            }

            if (areValid && string.Equals(currentPassword, newPassword))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextPasswordSameAsOld);
                areValid = false;
            }

            CheckFormStatusAndToggleSaveButton();
            
            return areValid;
        }

        private TextBlock GetErrorTextBlock(Control field)
        {
            if (field == txtCurrentPassword)
            { 
                return blckCurrentError; 
            }
            
            if (field == txtPassword)
            {
                return blckPasswordError;
            }
            
            if (field == txtPasswordConfirm)
            {
                return blckPasswordConfirmError;
            }

            return null;
        }

        private void ClearAllErrors()
        {
            ErrorDisplayService.ClearError(txtCurrentPassword, blckCurrentError);
            ErrorDisplayService.ClearError(txtPassword, blckPasswordError);
            ErrorDisplayService.ClearError(txtPasswordConfirm, blckPasswordConfirmError);
        }

        private void CheckFormStatusAndToggleSaveButton()
        {
            bool IsErrorRed(TextBlock block) =>
                !string.IsNullOrWhiteSpace(block.Text) &&
                (block.Foreground as SolidColorBrush)?.Color == Colors.Red;

            bool hasErrorMessages = IsErrorRed(blckCurrentError)
                                    || IsErrorRed(blckPasswordError)
                                    || IsErrorRed(blckPasswordConfirmError);

            bool allFieldsFilled = !string.IsNullOrWhiteSpace(txtCurrentPassword.Password)
                                   && !string.IsNullOrWhiteSpace(txtPassword.Password)
                                   && !string.IsNullOrWhiteSpace(txtPasswordConfirm.Password);

            btnSave.IsEnabled = !hasErrorMessages && allFieldsFilled;
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            string currentPassword = txtCurrentPassword.Password.Trim();
            string newPassword = txtPassword.Password.Trim();
            string confirmPassword = txtPasswordConfirm.Password.Trim();

            ErrorDisplayService.ClearError(passwordBox, GetErrorTextBlock(passwordBox));

            bool validationFailed = false;

            if (passwordBox == txtCurrentPassword)
            {
                if (!FieldValidator.IsLengthInRange(currentPassword, CURRENT_PASSWORD_MIN_LENGTH, PASSWORD_MAX_LENGTH))
                {
                    ErrorDisplayService.ShowError(txtCurrentPassword, blckCurrentError, Lang.DialogTextShortPassword);
                    validationFailed = true;
                }
            }
            else if (passwordBox == txtPassword)
            {
                if (!PasswordValidator.ValidateLength(newPassword, NEW_PASSWORD_MIN_LENGTH, PASSWORD_MAX_LENGTH))
                {
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextShortPassword);
                    validationFailed = true;
                }
                else if (!PasswordValidator.IsComplex(newPassword))
                {
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.GlobalTextPasswordNoComplex);
                    validationFailed = true;
                }
                else if (string.Equals(currentPassword, newPassword))
                {
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextPasswordSameAsOld);
                    validationFailed = true;
                }
            }

            if (FieldValidator.IsRequired(newPassword) && FieldValidator.IsRequired(confirmPassword))
            {
                if (!PasswordValidator.AreMatching(newPassword, confirmPassword))
                {
                    string errorMessage = Lang.DialogTextPasswordsDontMatch;
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, errorMessage);
                    ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, errorMessage);
                }
                else if (!validationFailed)
                {
                    ErrorDisplayService.ClearError(txtPassword, blckPasswordError);
                    ErrorDisplayService.ClearError(txtPasswordConfirm, blckPasswordConfirmError);
                }
            }

            SyncVisiblePasswordFields(passwordBox);

            CheckFormStatusAndToggleSaveButton();
        }

        private void SyncVisiblePasswordFields(PasswordBox passwordBox)
        {
            if (passwordBox == txtCurrentPassword && txtVisibleCurrentPassword.Visibility == Visibility.Visible)
            {
                txtVisibleCurrentPassword.Text = txtCurrentPassword.Password;
            }
            else if (passwordBox == txtPassword && txtVisiblePassword.Visibility == Visibility.Visible)
            {
                txtVisiblePassword.Text = txtPassword.Password;
            }
            else if (passwordBox == txtPasswordConfirm && txtVisiblePasswordConfirm.Visibility == Visibility.Visible)
            {
                txtVisiblePasswordConfirm.Text = txtPasswordConfirm.Password;
            }
        }

        private void PasswordLostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            string password = txtPassword.Password.Trim();
            string passwordConfirm = txtPasswordConfirm.Password.Trim();

            if (!FieldValidator.IsRequired(passwordBox.Password))
            {
                ErrorDisplayService.ShowError(passwordBox, GetErrorTextBlock(passwordBox), Lang.GlobalTextRequieredField);
            }

            if (FieldValidator.IsRequired(password) && FieldValidator.IsRequired(passwordConfirm) && !PasswordValidator.AreMatching(password, passwordConfirm))
            {
                string errorMessage = Lang.DialogTextPasswordsDontMatch;
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, errorMessage);
                ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, errorMessage);
            }

            CheckFormStatusAndToggleSaveButton();
        }

        private void ClickToggleVisibility(object sender, RoutedEventArgs e)
        {
            if (sender == btnToggleVisibilityCurrent)
            {
                PasswordVisibilityService.ToggleVisibility(txtCurrentPassword, txtVisibleCurrentPassword, blckEyeEmojiCurrent);

                Control visibleBox = txtVisibleCurrentPassword.Visibility == Visibility.Visible ? (Control)txtVisibleCurrentPassword : (Control)txtCurrentPassword;
                Control hiddenBox = visibleBox == txtVisibleCurrentPassword ? (Control)txtCurrentPassword : (Control)txtVisibleCurrentPassword;
                visibleBox.BorderBrush = hiddenBox.BorderBrush;
            }
            else if (sender == btnToggleVisibility)
            {
                PasswordVisibilityService.ToggleVisibility(txtPassword, txtVisiblePassword, blckEyeEmoji);

                Control visibleBox = txtVisiblePassword.Visibility == Visibility.Visible ? (Control)txtVisiblePassword : (Control)txtPassword;
                Control hiddenBox = visibleBox == txtVisiblePassword ? (Control)txtPassword : (Control)txtVisiblePassword;
                visibleBox.BorderBrush = hiddenBox.BorderBrush;
            }
            else if (sender == btnToggleVisibilityConfirm)
            {
                PasswordVisibilityService.ToggleVisibility(txtPasswordConfirm, txtVisiblePasswordConfirm, blckEyeEmojiConfirm);

                Control visibleBox = txtVisiblePasswordConfirm.Visibility == Visibility.Visible ? (Control)txtVisiblePasswordConfirm : (Control)txtPasswordConfirm;
                Control hiddenBox = visibleBox == txtVisiblePasswordConfirm ? (Control)txtPasswordConfirm : (Control)txtVisiblePasswordConfirm;
                visibleBox.BorderBrush = hiddenBox.BorderBrush;
            }
        }

        private void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == txtCurrentPassword)
                {
                    txtPassword.Focus();
                }
                else if (sender == txtPassword)
                {
                    txtPasswordConfirm.Focus();
                }
                else if (sender == txtPasswordConfirm && btnSave.IsEnabled)
                {
                    ClickSave(btnSave, null);
                }

                e.Handled = true;
            }
        }
    }
}