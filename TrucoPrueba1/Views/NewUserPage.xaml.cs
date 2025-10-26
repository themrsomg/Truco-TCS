using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrucoPrueba1.Properties.Langs;
using TrucoPrueba1.TrucoServer;

namespace TrucoPrueba1
{
    public partial class NewUserPage : Page
    {
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
                MessageBoxResult messageBoxResult = MessageBox.Show(Lang.DialogTextConfirmationNewUser, Lang.GlobalTextConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);

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

            if (email.Length < 5)
            {
                ShowError(txtEmail, Lang.DialogTextShortEmail);
                areValid = false;
            }
            else if (email.Length > 250)
            {
                ShowError(txtEmail, Lang.DialogTextLongEmail);
                areValid = false;
            }
            else if (!IsValidEmail(email))
            {
                ShowError(txtEmail, Lang.GlobalTextInvalidEmail);
                areValid = false;
            }

            if (username.Length < 4)
            {
                ShowError(txtUsername, Lang.DialogTextShortUsername);
                areValid = false;
            }
            else if (username.Length > 20)
            {
                ShowError(txtUsername, Lang.DialogTextLongUsername);
                areValid = false;
            }

            if (password.Length < 8)
            {
                ShowError(txtPassword, Lang.DialogTextShortPassword);
                areValid = false;
            }
            else if (password.Length > 50)
            {
                ShowError(txtPassword, Lang.DialogTextLongPassword);
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

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
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

        private void ClearSpecificError(Control field)
        {
            TextBlock errorBlock = GetErrorTextBlock(field);

            if (errorBlock != null)
            {
                errorBlock.Text = " ";
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
                if (text.Length < 5)
                {
                    ShowError(txtEmail, Lang.DialogTextShortEmail);
                }
                else if (text.Length > 250)
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
                if (text.Length < 4)
                {
                    ShowError(txtUsername, Lang.DialogTextShortUsername);
                }
                else if (text.Length > 20)
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

            if (password.Length < 8)
            {
                ShowError(passwordBox, Lang.DialogTextShortPassword);
            }
            else if (password.Length > 50)
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
            bool hasErrorMessages = blckEmailError.Text.Trim().Length > 0 ||
                                    blckUsernameError.Text.Trim().Length > 0 ||
                                    blckPasswordError.Text.Trim().Length > 0 ||
                                    blckPasswordConfirmError.Text.Trim().Length > 0;

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
