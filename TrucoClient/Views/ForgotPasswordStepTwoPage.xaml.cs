using System;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class ForgotPasswordStepTwoPage : Page
    {
        private const int CODE_LENGTH = 6;
        private const int MIN_PASSWORD_LENGTH = 12;
        private const int MAX_PASSWORD_LENGTH = 50;
        private const string MESSAGE_ERROR = "Error";

        private static readonly Regex verificationCodeRegex = new Regex(@"^[0-9]*$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(500));
        private static readonly Regex passwordAllowedRegex = new Regex(@"^[A-Za-z\d@$!%*?&.#_+=\-]*$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(500));

        private string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        private string email;

        public ForgotPasswordStepTwoPage(string email)
        {
            InitializeComponent();
            this.email = email ?? string.Empty;
            MusicInitializer.InitializeMenuMusic();
            blckVerificationCodeText.Text = string.Format(Lang.ForgotPasswordTextSent, email);
            InitializeValidation();
        }

        private void ClickSave(object sender, RoutedEventArgs e)
        {
            string code = txtVerificationCode.Text.Trim();
            string password = txtPassword.Password.Trim();
            string passwordConfirm = txtPasswordConfirm.Password.Trim();

            ClearAllErrors();
            bool codeOk = ValidateCode(code);
            bool passwordOk = ValidatePassword(password);
            bool confirmOk = ValidatePasswordConfirm(password, passwordConfirm);

            CheckFormStatusAndToggleSaveButton();

            if (!codeOk || !passwordOk || !confirmOk)
            {
                return;
            }

            if (InputSanitizer.ContainsDangerousCharacters(code) ||
                InputSanitizer.ContainsDangerousCharacters(password) ||
                InputSanitizer.ContainsDangerousCharacters(passwordConfirm))
            {
                CustomMessageBox.Show(Lang.DialogTextInvalidCharacters, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnSave.IsEnabled = false;

            try
            {
                var userClient = ClientManager.UserClient;
                var passwordResetOptions = new PasswordResetOptions
                {
                    Email = email,
                    Code = code,
                    NewPassword = password,
                    LanguageCode = languageCode

                };
                bool success = userClient.PasswordReset(passwordResetOptions);

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
            catch (EndpointNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(ClickSave));
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError, 
                    Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                ClientException.HandleError(ex, nameof(ClickSave));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException ex)
            {
                ClientException.HandleError(ex, nameof(ClickSave));
                CustomMessageBox.Show(Lang.ExceptionTextTimeout, MESSAGE_ERROR, 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(ClickSave));
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

        private void InitializeValidation()
        {
            try
            {
                InputRestriction.AttachRegexValidation(txtVerificationCode, verificationCodeRegex);

                InputRestriction.AttachRegexValidation(txtPassword, passwordAllowedRegex);
                InputRestriction.AttachRegexValidation(txtPasswordConfirm, passwordAllowedRegex);

                InputRestriction.AttachRegexValidation(txtVisiblePassword, passwordAllowedRegex);
                InputRestriction.AttachRegexValidation(txtVisiblePasswordConfirm, passwordAllowedRegex);
            }
            catch (ArgumentNullException ex)
            {
                ClientException.HandleError(ex, nameof(InitializeValidation));
            }
            catch (ArgumentException ex)
            {
                ClientException.HandleError(ex, nameof(InitializeValidation));
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(InitializeValidation));
            }
        }

        private bool ValidateCode(string code)
        {
            if (!FieldValidator.IsRequired(code))
            {
                ErrorDisplayService.ShowError(txtVerificationCode, blckCodeError, Lang.GlobalTextRequieredField);
                return false;
            }

            if (!FieldValidator.IsExactLength(code, CODE_LENGTH))
            {
                ErrorDisplayService.ShowError(txtVerificationCode, blckCodeError, Lang.GlobalTextVerificationCodeLength);
                return false;
            }

            if (!verificationCodeRegex.IsMatch(code))
            {
                ErrorDisplayService.ShowError(txtVerificationCode, blckCodeError, Lang.GlobalTextVerificationCodeNumber);
                return false;
            }

            ErrorDisplayService.ClearError(txtVerificationCode, blckCodeError);
            
            return true;
        }

        private bool ValidatePassword(string password)
        {
            if (!FieldValidator.IsRequired(password))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.GlobalTextRequieredField);
                
                return false;
            }

            if (!PasswordValidator.ValidateLength(password, MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextShortPassword);
                
                return false;
            }

            if (!PasswordValidator.IsComplex(password))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.GlobalTextPasswordNoComplex);
                
                return false;
            }

            ErrorDisplayService.ClearError(txtPassword, blckPasswordError);
            
            return true;
        }

        private bool ValidatePasswordConfirm(string password, string confirm)
        {
            if (!FieldValidator.IsRequired(confirm))
            {
                ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, Lang.GlobalTextRequieredField);
                
                return false;
            }

            if (!PasswordValidator.AreMatching(password, confirm))
            {
                string errorMessage = Lang.DialogTextPasswordsDontMatch;
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, errorMessage);
                ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, errorMessage);
                
                return false;
            }

            ErrorDisplayService.ClearError(txtPasswordConfirm, blckPasswordConfirmError);
            
            return true;
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

        private void EnterKeyDown(object sender, KeyEventArgs e)
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
    }
}