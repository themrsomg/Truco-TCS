using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class NewUserPage : Page
    {
        private const string MESSAGE_ERROR = "Error";
        private const int MIN_PASSWORD_LENGTH = 12;
        private const int MAX_PASSWORD_LENGTH = 50;
        private const int MIN_USERNAME_LENGTH = 4;
        private const int MAX_USERNAME_LENGTH = 20;
        private const int MIN_EMAIL_LENGTH = 5;
        private const int MAX_EMAIL_LENGTH = 250;

        private string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        public NewUserPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickRegister(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password.Trim();
            string passwordConfirm = txtPasswordConfirm.Password.Trim();
            string username = txtUsername.Text.Trim();

            ClearAllErrors();

            if (!FieldsValidation(email, password, passwordConfirm, username))
            {
                return;
            }

            try
            {
                var userClient = ClientManager.UserClient;

                if (EmailOrUsernameExists(email, username, userClient))
                {
                    return;
                }

                if (!RequestedEmail(email, userClient))
                {
                    return;
                }

                string code = ShowCodeInputWindow();
                if (string.IsNullOrEmpty(code))
                {
                    return;
                }

                if (!ConfirmedEmail(email, code, userClient))
                {
                    return;
                }

                bool registered = userClient.Register(username, password, email);

                if (registered)
                {
                    CustomMessageBox.Show(Lang.StartTextRegisterSuccess, Lang.GlobalTextSuccess, 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.NavigationService.Navigate(new LogInPage());
                }
                else
                {
                    CustomMessageBox.Show(Lang.StartTextRegisterError, MESSAGE_ERROR, 
                        MessageBoxButton.OK, MessageBoxImage.Error);
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
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            if (!HasUnsavedFields())
            {
                bool? result = CustomMessageBox.Show(
                    Lang.DialogTextConfirmationNewUser, 
                    Lang.GlobalTextConfirmation,        
                    MessageBoxButton.YesNo,             
                    MessageBoxImage.Question            
                );

                if (result == true)
                {
                    this.NavigationService.Navigate(new StartPage());
                }
            }
            else
            {
                this.NavigationService.Navigate(new StartPage());
            }
        }

        private bool FieldsValidation(string email, string password, string confirm, string username)
        {
            bool emailValid = ValidateEmail(email);
            bool usernameValid = ValidateUsername(username);
            bool passwordValid = ValidatePassword(password);
            bool confirmValid = ValidatePasswordConfirmation(password, confirm);

            CheckFormStatusAndToggleRegisterButton();
            return emailValid && usernameValid && passwordValid && confirmValid;
        }

        private bool ValidateEmail(string email)
        {
            if (!FieldValidator.IsRequired(email))
            {
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, Lang.GlobalTextRequieredField);
                return false;
            }
            if (!FieldValidator.IsLengthInRange(email, MIN_EMAIL_LENGTH, MAX_EMAIL_LENGTH))
            {
                string error = email.Length < MIN_EMAIL_LENGTH ? Lang.DialogTextShortEmail : Lang.DialogTextLongEmail;
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, error);
                return false;
            }
            if (!EmailValidator.IsValidEmail(email))
            {
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, Lang.GlobalTextInvalidEmail);
                return false;
            }
            if (!EmailValidator.IsCommonDomain(email))
            {
                ErrorDisplayService.ShowWarning(txtEmail, blckEmailError, Lang.StartTextUncommonEmailDomain);
            }

            ErrorDisplayService.ClearError(txtEmail, blckEmailError);
            return true;
        }

        private bool ValidateUsername(string username)
        {
            if (!FieldValidator.IsRequired(username))
            {
                ErrorDisplayService.ShowError(txtUsername, blckUsernameError, Lang.GlobalTextRequieredField);
                return false;
            }
            if (!FieldValidator.IsLengthInRange(username, MIN_USERNAME_LENGTH, MAX_USERNAME_LENGTH))
            {
                string error = username.Length < MIN_USERNAME_LENGTH ? Lang.DialogTextShortUsername : Lang.DialogTextLongUsername;
                ErrorDisplayService.ShowError(txtUsername, blckUsernameError, error);
                return false;
            }
            if (!UsernameValidator.IsValidFormat(username))
            {
                ErrorDisplayService.ShowError(txtUsername, blckUsernameError, Lang.GlobalTextInvalidUsername);
                return false;
            }
            ErrorDisplayService.ClearError(txtUsername, blckUsernameError);
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

        private bool ValidatePasswordConfirmation(string password, string confirm)
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

        private static void ValidateEmailChange(TextBox textBox, TextBlock errorBlock, string text)
        {
            if (!FieldValidator.IsLengthInRange(text, MIN_EMAIL_LENGTH, MAX_EMAIL_LENGTH))
            {
                string error = text.Length < MIN_EMAIL_LENGTH ? Lang.DialogTextShortEmail : Lang.DialogTextLongEmail;
                ErrorDisplayService.ShowError(textBox, errorBlock, error);
            }
            else if (!EmailValidator.IsValidEmail(text))
            {
                ErrorDisplayService.ShowError(textBox, errorBlock, Lang.GlobalTextInvalidEmail);
            }
            else if (!EmailValidator.IsCommonDomain(text))
            {
                ErrorDisplayService.ShowWarning(textBox, errorBlock, Lang.StartTextUncommonEmailDomain);
            }
        }

        private static void ValidateUsernameChange(TextBox textBox, TextBlock errorBlock, string text)
        {
            if (!FieldValidator.IsLengthInRange(text, MIN_USERNAME_LENGTH, MAX_USERNAME_LENGTH))
            {
                string error = text.Length < MIN_USERNAME_LENGTH ? Lang.DialogTextShortUsername : Lang.DialogTextLongUsername;
                ErrorDisplayService.ShowError(textBox, errorBlock, error);
            }
            else if (!UsernameValidator.IsValidFormat(text))
            {
                ErrorDisplayService.ShowError(textBox, errorBlock, Lang.GlobalTextInvalidUsername);
            }
        }
        private void ValidatePasswordLengthAndComplexity(string password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                if (!PasswordValidator.ValidateLength(password, MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH))
                {
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextShortPassword);
                }
                else if (!PasswordValidator.IsComplex(password))
                {
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.GlobalTextPasswordNoComplex);
                }
                else if (string.IsNullOrWhiteSpace(blckPasswordError.Text))
                {
                    ErrorDisplayService.ClearError(txtPassword, blckPasswordError);
                }
            }
        }
        private void ValidatePasswordMatching(string password, string confirm)
        {
            if (FieldValidator.IsRequired(password) && FieldValidator.IsRequired(confirm))
            {
                if (!PasswordValidator.AreMatching(password, confirm))
                {
                    string errorMessage = Lang.DialogTextPasswordsDontMatch;
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, errorMessage);
                    ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, errorMessage);
                }
                else
                {
                    ErrorDisplayService.ClearError(txtPasswordConfirm, blckPasswordConfirmError);
                    ValidatePasswordLengthAndComplexity(password);
                }
            }
        }

        private bool EmailOrUsernameExists(string email, string username, TrucoUserServiceClient client)
        {
            bool eitherExists = false;

            bool emailExists = client.EmailExists(email);
            bool usernameExists = client.UsernameExists(username);

            if (emailExists)
            {
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, Lang.GlobalTextEmailAlreadyInUse);
                eitherExists = true;
            }

            if (usernameExists)
            {
                ErrorDisplayService.ShowError(txtUsername, blckUsernameError, Lang.GlobalTextUsernameAlreadyInUse);
                eitherExists = true;
            }

            CheckFormStatusAndToggleRegisterButton();
            return eitherExists;
        }

        private bool RequestedEmail(string email, TrucoUserServiceClient client)
        {
            bool sent = client.RequestEmailVerification(email, languageCode);

            if (!sent)
            {
                CustomMessageBox.Show(Lang.StartTextRegisterCodeSended, MESSAGE_ERROR, 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return sent;
        }

        private static string ShowCodeInputWindow()
        {
            string code = Microsoft.VisualBasic.Interaction.InputBox(Lang.StartTextRegisterIntroduceCode, Lang.StartTextRegisterEmailVerification, "");

            if (string.IsNullOrEmpty(code))
            {
                CustomMessageBox.Show(Lang.StartTextRegisterMustEnterCode, MESSAGE_ERROR, 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return code;
        }

        private static bool ConfirmedEmail(string email, string code, TrucoUserServiceClient client)
        {
            bool confirmed = client.ConfirmEmailVerification(email, code);
            if (!confirmed)
            {
                CustomMessageBox.Show(Lang.StartTextRegisterIncorrectCode, MESSAGE_ERROR, 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return confirmed;
        }

        private void SyncVisiblePasswordBoxes(PasswordBox changedBox)
        {
            if (changedBox == txtPassword && txtVisiblePassword.Visibility == Visibility.Visible)
            {
                txtVisiblePassword.Text = txtPassword.Password;
            }
            else if (changedBox == txtPasswordConfirm && txtVisiblePasswordConfirm.Visibility == Visibility.Visible)
            {
                txtVisiblePasswordConfirm.Text = txtPasswordConfirm.Password;
            }
        }

        private TextBlock GetErrorTextBlock(Control field)
        {
            if (field == txtEmail)
            {
                return blckEmailError;
            }
            if (field == txtUsername)
            {
                return blckUsernameError;
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
            ErrorDisplayService.ClearError(txtEmail, blckEmailError);
            ErrorDisplayService.ClearError(txtUsername, blckUsernameError);
            ErrorDisplayService.ClearError(txtPassword, blckPasswordError);
            ErrorDisplayService.ClearError(txtPasswordConfirm, blckPasswordConfirmError);
        }

        private bool HasUnsavedFields()
        {
            return string.IsNullOrEmpty(txtEmail.Text.Trim()) &&
                   string.IsNullOrEmpty(txtPassword.Password.Trim()) &&
                   string.IsNullOrEmpty(txtPasswordConfirm.Password.Trim()) &&
                   string.IsNullOrEmpty(txtUsername.Text.Trim());
        }

        private void CheckFormStatusAndToggleRegisterButton()
        {
            Func<TextBlock, bool> hasError = block =>
                !string.IsNullOrWhiteSpace(block.Text) &&
                (block.Foreground as SolidColorBrush)?.Color == Colors.Red;

            bool hasErrorMessages = hasError(blckEmailError)
                                   || hasError(blckUsernameError)
                                   || hasError(blckPasswordError)
                                   || hasError(blckPasswordConfirmError);

            bool allFieldsFilled = !string.IsNullOrWhiteSpace(txtEmail.Text)
                                   && !string.IsNullOrWhiteSpace(txtUsername.Text)
                                   && !string.IsNullOrWhiteSpace(txtPassword.Password)
                                   && !string.IsNullOrWhiteSpace(txtPasswordConfirm.Password);

            btnRegister.IsEnabled = !hasErrorMessages && allFieldsFilled;
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string text = textBox.Text.Trim();
            TextBlock errorBlock = GetErrorTextBlock(textBox);

            ErrorDisplayService.ClearError(textBox, errorBlock);

            if (string.IsNullOrEmpty(text))
            {
                CheckFormStatusAndToggleRegisterButton();
                return;
            }

            if (textBox == txtEmail)
            {
                ValidateEmailChange(textBox, errorBlock, text);
            }
            else if (textBox == txtUsername)
            {
                ValidateUsernameChange(textBox, errorBlock, text);
            }

            CheckFormStatusAndToggleRegisterButton();
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            string text = textBox.Text.Trim();
            TextBlock errorBlock = GetErrorTextBlock(textBox);

            if (!FieldValidator.IsRequired(text))
            {
                ErrorDisplayService.ShowError(textBox, errorBlock, Lang.GlobalTextRequieredField);
            }

            CheckFormStatusAndToggleRegisterButton();
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            string password = txtPassword.Password.Trim();
            string passwordConfirm = txtPasswordConfirm.Password.Trim();

            if (passwordBox == txtPassword)
            {
                ErrorDisplayService.ClearError(txtPassword, blckPasswordError);
            }
            else if (passwordBox == txtPasswordConfirm)
            {
                ErrorDisplayService.ClearError(txtPasswordConfirm, blckPasswordConfirmError);
            }

            if (string.IsNullOrEmpty(password) && string.IsNullOrEmpty(passwordConfirm))
            {
                CheckFormStatusAndToggleRegisterButton();
                return;
            }

            ValidatePasswordLengthAndComplexity(password);

            ValidatePasswordMatching(password, passwordConfirm);

            SyncVisiblePasswordBoxes(passwordBox);

            CheckFormStatusAndToggleRegisterButton();
        }

        private void PasswordLostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            TextBlock errorBlock = GetErrorTextBlock(passwordBox);

            string password = txtPassword.Password.Trim();
            string passwordConfirm = txtPasswordConfirm.Password.Trim();

            if (!FieldValidator.IsRequired(passwordBox.Password))
            {
                ErrorDisplayService.ShowError(passwordBox, errorBlock, Lang.GlobalTextRequieredField);
            }

            if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(passwordConfirm) 
                && !PasswordValidator.AreMatching(password, passwordConfirm))
            {
                string errorMessage = Lang.DialogTextPasswordsDontMatch;
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, errorMessage);
                ErrorDisplayService.ShowError(txtPasswordConfirm, blckPasswordConfirmError, errorMessage);
            }

            CheckFormStatusAndToggleRegisterButton();
        }

        private void ClickToggleVisibility(object sender, RoutedEventArgs e)
        {
            if (sender == btnToggleVisibility)
            {
                PasswordVisibilityService.ToggleVisibility(txtPassword, txtVisiblePassword, blckEyeEmoji);

                if (txtVisiblePassword.Visibility == Visibility.Visible)
                {
                    txtVisiblePassword.BorderBrush = txtPassword.BorderBrush;
                }
                else
                {
                    txtPassword.BorderBrush = txtVisiblePassword.BorderBrush;
                }
            }

            if (sender == btnToggleVisibilityConfirm)
            {
                PasswordVisibilityService.ToggleVisibility(txtPasswordConfirm, txtVisiblePasswordConfirm, blckEyeEmojiConfirm);

                if (txtVisiblePasswordConfirm.Visibility == Visibility.Visible)
                {
                    txtVisiblePasswordConfirm.BorderBrush = txtPasswordConfirm.BorderBrush;
                }
                else
                {
                    txtPasswordConfirm.BorderBrush = txtVisiblePasswordConfirm.BorderBrush;
                }
            }
        }

        private void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (btnRegister.IsEnabled)
                {
                    ClickRegister(btnRegister, null);
                }

                if (sender == txtEmail)
                {
                    txtUsername.Focus();
                    e.Handled = true;
                }
                else if (sender == txtUsername)
                {
                    txtPassword.Focus();
                    e.Handled = true;
                }
                else if (sender == txtPassword)
                {
                    txtPasswordConfirm.Focus();
                    e.Handled = true;
                }
                else if (sender == txtPasswordConfirm && btnRegister.IsEnabled)
                {
                    ClickRegister(btnRegister, null);
                }
            }
        }

        private void UsernamePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$", RegexOptions.None, TimeSpan.FromMilliseconds(100));
        }
    }
}