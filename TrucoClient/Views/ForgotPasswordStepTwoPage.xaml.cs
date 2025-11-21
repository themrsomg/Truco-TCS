using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;

namespace TrucoClient.Views
{
    public partial class ForgotPasswordStepTwoPage : Page
    {
        private const int CODE_LENGTH = 6;
        private const int MIN_PASSWORD_LENGTH = 8;
        private const int MAX_PASSWORD_LENGTH = 50;
        private const string MESSAGE_ERROR = "Error";

        private string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        private string email;

        public ForgotPasswordStepTwoPage(string email)
        {
            InitializeComponent();
            this.email = email;
            MusicInitializer.InitializeMenuMusic();
            blckVerificationCodeText.Text = string.Format(Lang.ForgotPasswordTextSent, email);
        }

        private void ClickSave(object sender, RoutedEventArgs e)
        {
            string code = txtVerificationCode.Text.Trim();
            string password = txtPassword.Password.Trim();
            string passwordConfirm = txtPasswordConfirm.Password.Trim();

            ClearAllErrors();
            if (!FieldsValidation(code, password, passwordConfirm))
            {
                return;
            }

            btnSave.IsEnabled = false;

            try
            {
                var userClient = ClientManager.UserClient;
                bool success = userClient.PasswordReset(email, code, password, languageCode);

                if (success)
                {
                    CustomMessageBox.Show(Lang.ForgotPasswordTextSuccess, Lang.GlobalTextSuccess, 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.NavigationService.Navigate(new LogInPage());
                }
                else
                {
                    CustomMessageBox.Show(Lang.ForgotPasswordTextError, MESSAGE_ERROR, 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (System.ServiceModel.EndpointNotFoundException)
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
            this.NavigationService.Navigate(new StartPage());
        }

        private void EnterKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == txtVerificationCode)
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

        private bool FieldsValidation(string code, string password, string passwordConfirm)
        {
            bool areValid = true;

            if (!FieldValidator.IsRequired(code))
            {
                ErrorDisplayService.ShowError(txtVerificationCode, blckCodeError, Lang.GlobalTextRequieredField);
                areValid = false;
            }
            else if (!FieldValidator.IsExactLength(code, CODE_LENGTH))
            {
                ErrorDisplayService.ShowError(txtVerificationCode, blckCodeError, Lang.GlobalTextVerificationCodeLength);
                areValid = false;
            }
            else if (!FieldValidator.TryParseInt(code))
            {
                ErrorDisplayService.ShowError(txtVerificationCode, blckCodeError, Lang.GlobalTextVerificationCodeNumber);
                areValid = false;
            }

            if (!FieldValidator.IsRequired(password))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.GlobalTextRequieredField);
                areValid = false;
            }
            else if (!PasswordValidator.ValidateLength(password, MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextShortPassword);
                areValid = false;
            }
            else if (!PasswordValidator.IsComplex(password))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.GlobalTextPasswordNoComplex);
                areValid = false;
            }

            if (!FieldValidator.IsRequired(passwordConfirm))
            {
                ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, Lang.GlobalTextRequieredField);
                areValid = false;
            }
            else if (!PasswordValidator.AreMatching(password, passwordConfirm))
            {
                string errorMessage = Lang.DialogTextPasswordsDontMatch;
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, errorMessage);
                ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, errorMessage);
                areValid = false;
            }

            CheckFormStatusAndToggleSaveButton();
            return areValid;
        }

        private TextBlock GetErrorTextBlock(Control field)
        {
            if (field == txtVerificationCode)
            {
                return blckCodeError;
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
            ErrorDisplayService.ClearError(txtVerificationCode, blckCodeError);
            ErrorDisplayService.ClearError(txtPassword, blckPasswordError);
            ErrorDisplayService.ClearError(txtPasswordConfirm, blckPasswordConfirmError);
        }

        private void CheckFormStatusAndToggleSaveButton()
        {
            bool IsErrorRed(TextBlock block) =>
                !string.IsNullOrWhiteSpace(block.Text) && 
                (block.Foreground as SolidColorBrush)?.Color == Colors.Red;

            bool hasErrorMessages = IsErrorRed(blckCodeError)
                                    || IsErrorRed(blckPasswordError)
                                    || IsErrorRed(blckPasswordConfirmError);

            bool allFieldsFilled = !string.IsNullOrWhiteSpace(txtVerificationCode.Text)
                                   && !string.IsNullOrWhiteSpace(txtPassword.Password)
                                   && !string.IsNullOrWhiteSpace(txtPasswordConfirm.Password);

            btnSave.IsEnabled = !hasErrorMessages && allFieldsFilled;
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            ErrorDisplayService.ClearError(textBox, GetErrorTextBlock(textBox));
            string text = textBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                CheckFormStatusAndToggleSaveButton();
                return;
            }

            if (textBox == txtVerificationCode)
            {
                if (!FieldValidator.IsExactLength(text, CODE_LENGTH))
                {
                    ErrorDisplayService.ShowError(txtVerificationCode, blckCodeError, Lang.GlobalTextVerificationCodeLength);
                }
                else if (!FieldValidator.TryParseInt(text))
                {
                    ErrorDisplayService.ShowError(txtVerificationCode, blckCodeError, Lang.GlobalTextVerificationCodeNumber);
                }
            }

            CheckFormStatusAndToggleSaveButton();
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtVerificationCode.Text))
            {
                ErrorDisplayService.ShowError(txtVerificationCode, blckCodeError, Lang.GlobalTextRequieredField);
            }
            CheckFormStatusAndToggleSaveButton();
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            ErrorDisplayService.ClearError(passwordBox, GetErrorTextBlock(passwordBox));

            string password = txtPassword.Password;
            string passwordConfirm = txtPasswordConfirm.Password;
            bool validationFailed = false;

            if (passwordBox == txtPassword && !string.IsNullOrEmpty(password))
            {
                if (!PasswordValidator.ValidateLength(password, MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH))
                {
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextShortPassword);
                    validationFailed = true;
                }
                else if (!PasswordValidator.IsComplex(password))
                {
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.GlobalTextPasswordNoComplex);
                    validationFailed = true;
                }
            }

            if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(passwordConfirm))
            {
                if (!PasswordValidator.AreMatching(password, passwordConfirm))
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

        private void PasswordLostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            if (!FieldValidator.IsRequired(passwordBox.Password))
            {
                ErrorDisplayService.ShowError(passwordBox, GetErrorTextBlock(passwordBox), Lang.GlobalTextRequieredField);
            }

            string password = txtPassword.Password;
            string passwordConfirm = txtPasswordConfirm.Password;
            if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(passwordConfirm) && !PasswordValidator.AreMatching(password, passwordConfirm))
            {
                string errorMessage = Lang.DialogTextPasswordsDontMatch;
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, errorMessage);
                ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, errorMessage);
            }

            CheckFormStatusAndToggleSaveButton();
        }

        private void ClickToggleVisibility(object sender, RoutedEventArgs e)
        {
            if (sender == btnToggleVisibility)
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

        private void SyncVisiblePasswordFields(PasswordBox passwordBox)
        {
            if (passwordBox == txtPassword && txtVisiblePassword.Visibility == Visibility.Visible)
            {
                txtVisiblePassword.Text = txtPassword.Password;
            }
            else if (passwordBox == txtPasswordConfirm && txtVisiblePasswordConfirm.Visibility == Visibility.Visible)
            {
                txtVisiblePasswordConfirm.Text = txtPasswordConfirm.Password;
            }
        }
    }
}