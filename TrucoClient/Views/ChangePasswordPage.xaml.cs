using System;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class ChangePasswordPage : Page
    {
        private const int NEW_PASSWORD_MIN_LENGTH = 12;
        private const int PASSWORD_MAX_LENGTH = 50;
        private const int CURRENT_PASSWORD_MIN_LENGTH = 8;
        private const string MESSAGE_ERROR = "Error";

        private static readonly Regex passwordInputRegex = new Regex(@"^[A-Za-z\d@$!%*?&.#_+=\-]*$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(500));

        private readonly string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        public ChangePasswordPage()
        {
            InitializeComponent();
            InitializeValidation();
            MusicInitializer.InitializeMenuMusic();
        }

        private void InitializeValidation()
        {
            InputRestriction.AttachRegexValidation(txtCurrentPassword, passwordInputRegex);
            InputRestriction.AttachRegexValidation(txtPassword, passwordInputRegex);
            InputRestriction.AttachRegexValidation(txtPasswordConfirm, passwordInputRegex);
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
            if (HasUnsavedFields())
            {
                bool? result = CustomMessageBox.Show(Lang.DialogTextConfirmationNewUser, Lang.GlobalTextConfirmation,
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == true)
                {
                    this.NavigationService.Navigate(new UserProfilePage());
                }
            }
            else
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
            return !string.IsNullOrEmpty(txtCurrentPassword.Password.Trim()) ||
                   !string.IsNullOrEmpty(txtPassword.Password.Trim()) ||
                   !string.IsNullOrEmpty(txtPasswordConfirm.Password.Trim());
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
            else if (string.Equals(currentPassword, newPassword))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextPasswordSameAsOld);
                areValid = false;
            }

            if (!FieldValidator.IsRequired(confirmPassword))
            {
                ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, Lang.GlobalTextRequieredField);
                areValid = false;
            }
            else if (!PasswordValidator.AreMatching(newPassword, confirmPassword))
            {
                string errorMsg = Lang.DialogTextPasswordsDontMatch;
                ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, errorMsg);
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
            if (passwordBox == null)
            {
                return;
            }

            string currentPassword = txtCurrentPassword.Password.Trim();
            string newPassword = txtPassword.Password.Trim();
            string confirmPassword = txtPasswordConfirm.Password.Trim();

            ErrorDisplayService.ClearError(passwordBox, GetErrorTextBlock(passwordBox));

            bool isCurrentValid = true;

            if (passwordBox == txtCurrentPassword)
            {
                if (!FieldValidator.IsLengthInRange(currentPassword, CURRENT_PASSWORD_MIN_LENGTH, PASSWORD_MAX_LENGTH))
                {
                    ErrorDisplayService.ShowError(txtCurrentPassword, blckCurrentError, Lang.DialogTextShortPassword);
                }
            }
            else if (passwordBox == txtPassword)
            {
                if (!PasswordValidator.ValidateLength(newPassword, NEW_PASSWORD_MIN_LENGTH, PASSWORD_MAX_LENGTH))
                {
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextShortPassword);
                    isCurrentValid = false;
                }
                else if (!PasswordValidator.IsComplex(newPassword))
                {
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.GlobalTextPasswordNoComplex);
                    isCurrentValid = false;
                }
                else if (string.Equals(currentPassword, newPassword) && !string.IsNullOrEmpty(currentPassword))
                {
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextPasswordSameAsOld);
                    isCurrentValid = false;
                }
            }

            if (!string.IsNullOrEmpty(newPassword) && !string.IsNullOrEmpty(confirmPassword))
            {
                if (!PasswordValidator.AreMatching(newPassword, confirmPassword))
                {
                    string msg = Lang.DialogTextPasswordsDontMatch;
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, msg);
                    ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, msg);
                }
                else if (isCurrentValid)
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
            if (passwordBox == null) return;

            if (!FieldValidator.IsRequired(passwordBox.Password))
            {
                ErrorDisplayService.ShowError(passwordBox, GetErrorTextBlock(passwordBox), Lang.GlobalTextRequieredField);
            }

            CheckFormStatusAndToggleSaveButton();
        }

        private void ClickToggleVisibility(object sender, RoutedEventArgs e)
        {
            PasswordBox passBox = null;
            TextBox textBox = null;
            TextBlock icon = null;

            if (sender == btnToggleVisibilityCurrent)
            {
                passBox = txtCurrentPassword;
                textBox = txtVisibleCurrentPassword;
                icon = blckEyeEmojiCurrent;
            }
            else if (sender == btnToggleVisibility)
            {
                passBox = txtPassword;
                textBox = txtVisiblePassword;
                icon = blckEyeEmoji;
            }
            else if (sender == btnToggleVisibilityConfirm)
            {
                passBox = txtPasswordConfirm;
                textBox = txtVisiblePasswordConfirm;
                icon = blckEyeEmojiConfirm;
            }

            if (passBox != null)
            {
                PasswordVisibilityService.ToggleVisibility(passBox, textBox, icon);

                Control visibleBox = textBox.Visibility == Visibility.Visible ? (Control)textBox : (Control)passBox;
                Control hiddenBox = visibleBox == textBox ? (Control)passBox : (Control)textBox;
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
