using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient
{
    public partial class NewUserPage : Page
    {
        private const int MIN_PASSWORD_LENGTH = 12;
        private const int MAX_PASSWORD_LENGTH = 50;
        private const int MIN_USERNAME_LENGTH = 4;
        private const int MAX_USERNAME_LENGTH = 20;
        private const int MIN_EMAIL_LENGTH = 5;
        private const int MAX_EMAIL_LENGTH = 250;
        private const int MIN_TEXT_LENGTH = 5;
        private const int MAX_TEXT_LENGTH = 250;

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
                    MessageBox.Show(Lang.StartTextRegisterSuccess, Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                    this.NavigationService.Navigate(new LogInPage());
                }
                else
                {
                    MessageBox.Show(Lang.StartTextRegisterError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            if (!HasUnsavedFields())
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(Lang.DialogTextConfirmationNewUser, Lang.GlobalTextConfirmation,
                MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    this.NavigationService.Navigate(new StartPage());
                }
                else
                {
                    return;
                }
            }
            else
            {
                this.NavigationService.Navigate(new StartPage());
            }
        }

        private void EnterKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if(btnRegister.IsEnabled)
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
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$");
        }

        private bool HasUnsavedFields()
        {
            bool canGoBack = false;

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password.Trim();
            string passwordConfirm = txtPasswordConfirm.Password.Trim();
            string username = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(passwordConfirm) ||
                string.IsNullOrEmpty(username))
            {
                canGoBack = true;
            }

            return canGoBack;
        }

        private bool FieldsValidation(string email, string password, string passwordConfirm, string username)
        {
            ClearAllErrors();
            bool areValid = true;

            if (string.IsNullOrEmpty(email))
            {
                ShowError(txtEmail, Lang.GlobalTextRequieredField);
                areValid = false;
            }

            if (string.IsNullOrEmpty(username))
            {
                ShowError(txtUsername, Lang.GlobalTextRequieredField);
                areValid = false;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError(txtPassword, Lang.GlobalTextRequieredField);
                areValid = false;
            }

            if (string.IsNullOrEmpty(passwordConfirm))
            {
                ShowError(txtPasswordConfirm, Lang.GlobalTextRequieredField);
                areValid = false;
            }

            if (!areValid)
            {
                return false;
            }

            if (email.Length < MIN_EMAIL_LENGTH)
            {
                ShowError(txtEmail, Lang.DialogTextShortEmail);
                areValid = false;
            }
            else if (email.Length > MAX_EMAIL_LENGTH)
            {
                ShowError(txtEmail, Lang.DialogTextLongEmail);
                areValid = false;
            }
            else if (IsValidEmail(email))
            {
                if (!IsCommonEmailProvider(email))
                {
                    ShowWarning(txtEmail, Lang.StartTextUncommonEmailDomain);
                }
            }
            else
            {
                ShowError(txtEmail, Lang.GlobalTextInvalidEmail);
                areValid = false;
            }

            if (username.Length < MIN_USERNAME_LENGTH)
            {
                ShowError(txtUsername, Lang.DialogTextShortUsername);
                areValid = false;
            }
            else if (username.Length > MAX_USERNAME_LENGTH)
            {
                ShowError(txtUsername, Lang.DialogTextLongUsername);
                areValid = false;
            }
            else if (!IsValidUsername(username))
            {
                ShowError(txtUsername, Lang.GlobalTextInvalidUsername);
                areValid = false;
            }

            if (password.Length < MIN_PASSWORD_LENGTH)
            {
                ShowError(txtPassword, Lang.DialogTextShortPassword);
                areValid = false;
            }
            else if (password.Length > MAX_PASSWORD_LENGTH)
            {
                ShowError(txtPassword, Lang.DialogTextLongPassword);
                areValid = false;
            }
            else if (!IsPasswordComplex(password))
            {
                ShowError(txtPassword, Lang.GlobalTextPasswordNoComplex);
                areValid = false;
            }

            if (!string.Equals(password, passwordConfirm))
            {
                string errorMessage = Lang.DialogTextPasswordsDontMatch;
                ShowError(txtPassword, errorMessage);
                ShowError(txtPasswordConfirm, errorMessage);
                areValid = false;
            }

            CheckFormStatusAndToggleRegisterButton();
            return areValid;
        }

        private bool IsPasswordComplex(string password)
        {
            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;
            bool hasSymbol = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c))
                {
                    hasUpper = true;
                }
                else if (char.IsLower(c))
                {
                    hasLower = true;
                }
                else if (char.IsDigit(c))
                {
                    hasDigit = true;
                }
                else if (!char.IsLetterOrDigit(c))
                {
                    hasSymbol = true;
                }
            }

            return hasUpper && hasLower && hasDigit && hasSymbol;
        }

        private bool IsValidUsername(string username)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9]+$");
        }

        private bool IsValidEmail(string email)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(email))
            {
                isValid = false;
            }

            if (email.Contains("..") || email.Contains(" "))
            {
                isValid = false;
            }

            if (!isValid)
            {
                return isValid;
            }

            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                isValid = System.Text.RegularExpressions.Regex.IsMatch( email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[A-Za-z]{2,}$" ) 
                    && address.Address == email;

            }
            catch (FormatException)
            {
                return isValid;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Ocurrió un error al validar el correo electrónico: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ha ocurrido un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return isValid;
        }

        private bool IsCommonEmailProvider(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                return false;
            }

            var parts = email.Split('@');
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                return false;
            }

            string domain = parts[1].ToLower();

            string[] commonDomains =
            {
                "gmail.com", "outlook.com", "hotmail.com",
                "yahoo.com", "icloud.com", "live.com",
                "aol.com", "protonmail.com", "uv.mx", "estudiantes.uv.mx"
            };

            return commonDomains.Contains(domain);
        }

        private bool EmailOrUsernameExists(string email, string username, TrucoUserServiceClient client)
        {
            bool eitherExists = false;

            bool emailExists = client.EmailExists(email);
            bool usernameExists = client.UsernameExists(username);

            if (emailExists)
            {
                ShowError(txtEmail, Lang.GlobalTextEmailAlreadyInUse);
                eitherExists = true;
            }

            if (usernameExists)
            {
                ShowError(txtUsername, Lang.GlobalTextUsernameAlreadyInUse);
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
                MessageBox.Show(Lang.StartTextRegisterCodeSended, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return sent;
        }

        private string ShowCodeInputWindow()
        {
            string code = Microsoft.VisualBasic.Interaction.InputBox(Lang.StartTextRegisterIntroduceCode, Lang.StartTextRegisterEmailVerification, "");

            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show(Lang.StartTextRegisterMustEnterCode, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return code;
        }

        private bool ConfirmedEmail(string email, string code, TrucoUserServiceClient client)
        {
            bool confirmed = client.ConfirmEmailVerification(email, code);

            if (!confirmed)
            {
                MessageBox.Show(Lang.StartTextRegisterIncorrectCode, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return confirmed;
        }

        private TextBlock GetErrorTextBlock(Control field)
        {
           TextBlock errorBlock = null;

            if (field == txtEmail)
            {
                errorBlock = blckEmailError;
            }
            if (field == txtUsername)
            {
                errorBlock = blckUsernameError;
            }
            if (field == txtPassword)
            {
                errorBlock = blckPasswordError;
            }
            if (field == txtPasswordConfirm)
            {
                errorBlock = blckPasswordConfirmError;
            }

            return errorBlock;
        }

        private void ShowError(Control field, string errorMessage)
        {
            TextBlock errorBlock = GetErrorTextBlock(field);

            if (errorBlock != null)
            {
                errorBlock.Text = errorMessage;
            }

            field.BorderBrush = new SolidColorBrush(Colors.Red);
        }

        private void ShowWarning(Control field, string warningMessage)
        {
            TextBlock warningBlock = GetErrorTextBlock(field);

            if (warningBlock != null)
            {
                warningBlock.Text = warningMessage;
                warningBlock.Foreground = new SolidColorBrush(Colors.Orange);
            }
        }

        private void ClearSpecificError(Control field)
        {
            TextBlock errorBlock = GetErrorTextBlock(field);

            if (errorBlock != null)
            {
                errorBlock.Text = string.Empty;
                errorBlock.Foreground = new SolidColorBrush(Colors.Red);
            }

            field.ClearValue(Border.BorderBrushProperty);
        }

        private void ClearAllErrors()
        {
            ClearSpecificError(txtEmail);
            ClearSpecificError(txtUsername);
            ClearSpecificError(txtPassword);
            ClearSpecificError(txtPasswordConfirm);
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            ClearSpecificError(textBox);
            string text = textBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                CheckFormStatusAndToggleRegisterButton();
                return;
            }

            if (textBox == txtEmail)
            {
                if (text.Length < MIN_TEXT_LENGTH)
                {
                    ShowError(txtEmail, Lang.DialogTextShortEmail);
                }
                else if (text.Length > MAX_TEXT_LENGTH)
                {
                    ShowError(txtEmail, Lang.DialogTextLongEmail);
                }
                else if (!IsValidEmail(text))
                {
                    ShowError(txtEmail, Lang.GlobalTextInvalidEmail);
                }
            }
            else if (textBox == txtUsername)
            {
                if (text.Length < MIN_TEXT_LENGTH)
                {
                    ShowError(txtUsername, Lang.DialogTextShortUsername);
                }
                else if (text.Length > MAX_TEXT_LENGTH)
                {
                    ShowError(txtUsername, Lang.DialogTextLongUsername);
                }
            }

            CheckFormStatusAndToggleRegisterButton();
        }
        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                ShowError(textBox, Lang.GlobalTextRequieredField);
            }

            CheckFormStatusAndToggleRegisterButton();
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            ClearSpecificError(passwordBox);

            string password = passwordBox.Password.Trim();

            if (string.IsNullOrEmpty(password))
            {
                return;
            }

            if (password.Length < MIN_PASSWORD_LENGTH)
            {
                ShowError(passwordBox, Lang.DialogTextShortPassword);
            }
            else if (password.Length > MAX_PASSWORD_LENGTH)
            {
                ShowError(passwordBox, Lang.DialogTextLongPassword);
            }

            string password1 = txtPassword.Password;
            string password2 = txtPasswordConfirm.Password;

            if (!string.IsNullOrEmpty(password1) && !string.IsNullOrEmpty(password2) && !string.Equals(password1, password2))
            {
                string errorMessage = Lang.DialogTextPasswordsDontMatch;
                ShowError(txtPassword, errorMessage);
                ShowError(txtPasswordConfirm, errorMessage);
            }
            else if (!string.IsNullOrEmpty(password1) && !IsPasswordComplex(password1))
            {
                string errorMessage = Lang.GlobalTextPasswordNoComplex;
                ShowError(txtPassword, errorMessage);
                ShowError(txtPasswordConfirm, errorMessage);
            }
            else
            {
                if (string.Equals(password1, password2) && !string.IsNullOrEmpty(password1))
                {
                    ClearSpecificError(txtPassword);
                    ClearSpecificError(txtPasswordConfirm);
                }
            }

            if (passwordBox == txtPassword && txtVisiblePassword.Visibility == Visibility.Visible)
            {
                txtVisiblePassword.Text = txtPassword.Password;
            }
            else if (passwordBox == txtPasswordConfirm && txtVisiblePasswordConfirm.Visibility == Visibility.Visible)
            {
                txtVisiblePasswordConfirm.Text = txtPasswordConfirm.Password;
            }

            CheckFormStatusAndToggleRegisterButton();
        }

        private void PasswordLostFocus(object sender, RoutedEventArgs e)
        {
            string password = txtPassword.Password;
            string passwordConfirm = txtPasswordConfirm.Password;

            if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(passwordConfirm) && !string.Equals(password, passwordConfirm))
            {
                string errorMessage = Lang.DialogTextPasswordsDontMatch;
                ShowError(txtPassword, errorMessage);
                ShowError(txtPasswordConfirm, errorMessage);
            }

            var passwordBox = sender as PasswordBox;

            if (string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                ShowError(passwordBox, Lang.GlobalTextRequieredField);
            }

            CheckFormStatusAndToggleRegisterButton();
        }

        private void CheckFormStatusAndToggleRegisterButton()
        {
            bool hasErrorMessages = (!string.IsNullOrWhiteSpace(blckEmailError.Text) &&
                                     (blckEmailError.Foreground as SolidColorBrush)?.Color == Colors.Red)
                                    ||
                                    (!string.IsNullOrWhiteSpace(blckUsernameError.Text) &&
                                     (blckUsernameError.Foreground as SolidColorBrush)?.Color == Colors.Red)
                                    ||
                                    (!string.IsNullOrWhiteSpace(blckPasswordError.Text) &&
                                     (blckPasswordError.Foreground as SolidColorBrush)?.Color == Colors.Red)
                                    ||
                                    (!string.IsNullOrWhiteSpace(blckPasswordConfirmError.Text) &&
                                     (blckPasswordConfirmError.Foreground as SolidColorBrush)?.Color == Colors.Red);


            bool allFieldsFilled = !string.IsNullOrWhiteSpace(txtEmail.Text) &&
                                   !string.IsNullOrWhiteSpace(txtUsername.Text) &&
                                   !string.IsNullOrWhiteSpace(txtPassword.Password) &&
                                   !string.IsNullOrWhiteSpace(txtPasswordConfirm.Password);

            bool canEnable = !hasErrorMessages && allFieldsFilled;

            btnRegister.IsEnabled = canEnable;
        }

        private void ClickToggleVisibility(object sender, RoutedEventArgs e)
        {
            if (sender == btnToggleVisibility)
            {
                if (txtPassword.Visibility == Visibility.Visible)
                {
                    txtVisiblePassword.Text = txtPassword.Password;

                    txtPassword.Visibility = Visibility.Collapsed;
                    txtVisiblePassword.Visibility = Visibility.Visible;
                    txtVisiblePassword.Focus();

                    blckEyeEmoji.Foreground = new SolidColorBrush(Colors.White);
                    txtVisiblePassword.BorderBrush = txtPassword.BorderBrush;
                }
                else
                {
                    txtPassword.Password = txtVisiblePassword.Text;

                    txtPassword.Visibility = Visibility.Visible;
                    txtVisiblePassword.Visibility = Visibility.Collapsed;
                    txtPassword.Focus();

                    blckEyeEmoji.Foreground = new SolidColorBrush(Colors.Black);
                    txtVisiblePassword.BorderBrush = txtPassword.BorderBrush;
                }
                return;
            }

            if (sender == btnToggleVisibilityConfirm)
            {
                if (txtPasswordConfirm.Visibility == Visibility.Visible)
                {
                    txtVisiblePasswordConfirm.Text = txtPasswordConfirm.Password;

                    txtPasswordConfirm.Visibility = Visibility.Collapsed;
                    txtVisiblePasswordConfirm.Visibility = Visibility.Visible;
                    txtVisiblePasswordConfirm.Focus();

                    blckEyeEmojiConfirm.Foreground = new SolidColorBrush(Colors.White);
                    txtVisiblePasswordConfirm.BorderBrush = txtPassword.BorderBrush;
                }
                else
                {
                    txtPasswordConfirm.Password = txtVisiblePasswordConfirm.Text;

                    txtPasswordConfirm.Visibility = Visibility.Visible;
                    txtVisiblePasswordConfirm.Visibility = Visibility.Collapsed;
                    txtPasswordConfirm.Focus();

                    blckEyeEmojiConfirm.Foreground = new SolidColorBrush(Colors.Black);
                    txtVisiblePasswordConfirm.BorderBrush = txtPassword.BorderBrush;
                }
                return;
            }
        }
    }
}
