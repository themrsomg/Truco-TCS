using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrucoPrueba1.Properties.Langs;

namespace TrucoPrueba1.Views
{
    public partial class ForgotPasswordStepTwoPage : Page
    {
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

            if (!FieldsValidation(code, password))
            {
                return;
            }

            try
            {
                var userClient = ClientManager.UserClient;

                bool success = userClient.PasswordReset(email, code, password, languageCode);

                if (success)
                {
                    MessageBox.Show(Lang.ForgotPasswordTextSuccess, Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                    this.NavigationService.Navigate(new LogInPage());
                }
                else
                {
                    MessageBox.Show(Lang.ForgotPasswordTextError, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            this.NavigationService.Navigate(new StartPage());
        }

        private void EnterKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (btnSave.IsEnabled)
                {
                    ClickSave(btnSave, null);
                }
                if (sender == txtVerificationCode)
                {
                    txtPassword.Focus();
                    e.Handled = true;
                }
                else if (sender == txtPassword)
                {
                    txtPasswordConfirm.Focus();
                    e.Handled = true;
                }
                else if (sender == txtPasswordConfirm && btnSave.IsEnabled)
                {
                    ClickSave(btnSave, null);
                }
            }
        }

        private bool FieldsValidation(string code, string password)
        {
            ClearAllErrors();
            bool areValid = true;

            string passwordConfirm = txtPasswordConfirm.Password.Trim();

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

            if (string.IsNullOrEmpty(code))
            {
                ShowError(txtVerificationCode, Lang.GlobalTextRequieredField);
                areValid = false;
            }

            if (!areValid)
            {
                return false;
            }

            if (code.Length != 6)
            {
                ShowError(txtVerificationCode, Lang.GlobalTextVerificationCodeLength);
                areValid = false;
            }
            else if (!int.TryParse(code, out _))
            {
                ShowError(txtVerificationCode, Lang.GlobalTextVerificationCodeNumber);
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

        private TextBlock GetErrorTextBlock(Control field)
        {
            TextBlock errorBlock = null;

            if (field == txtVerificationCode)
            {
                errorBlock = blckCodeError;
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

            if (textBox == txtVerificationCode)
            {
                if (text.Length != 6)
                {
                    ShowError(txtVerificationCode, Lang.GlobalTextVerificationCodeLength);
                }
                else if (!int.TryParse(text, out _))
                {
                    ShowError(txtVerificationCode, Lang.GlobalTextVerificationCodeNumber);
                }
            }

            CheckFormStatusAndToggleRegisterButton();
        }
        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtVerificationCode.Text))
            {
                ShowError(txtVerificationCode, Lang.GlobalTextRequieredField);
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
            bool hasErrorMessages = blckPasswordError.Text.Trim().Length > 0 ||
                                    blckPasswordConfirmError.Text.Trim().Length > 0 || 
                                    blckCodeError.Text.Trim().Length > 0;

            bool allFieldsFilled = !string.IsNullOrWhiteSpace(txtPassword.Password) &&
                                   !string.IsNullOrWhiteSpace(txtPasswordConfirm.Password) &&
                                   !string.IsNullOrWhiteSpace(txtVerificationCode.Text);

            bool canEnable = !hasErrorMessages && allFieldsFilled;

            btnSave.IsEnabled = canEnable;
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
