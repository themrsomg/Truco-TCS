using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class ChangePasswordPage : Page
    {
        private const int PASSWORD_MIN_LENGTH = 12;
        private const int PASSWORD_MAX_LENGTH = 50;
        private const int MIN_LENGTH = 0;

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
                    MessageBox.Show(Lang.DialogTextPasswordChangedSuccess, Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                    this.NavigationService.Navigate(new LogInPage());
                }
                else
                {
                    MessageBox.Show(Lang.DialogTextPasswordChangeError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.ServiceModel.EndpointNotFoundException ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextConnectionError, ex.Message), Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorOcurred, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (this.IsLoaded)
                {
                    btnSave.IsEnabled = true;
                }
            }
        }
        private void ClickForgotPassword(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ForgotPasswordStepOnePage());
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            if (!HasUnsavedFields())
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(Lang.DialogTextConfirmationNewUser, Lang.GlobalTextConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    this.NavigationService.Navigate(new UserProfilePage());
                }
                else
                {
                    return;
                }
            }
            else
            {
                this.NavigationService.Navigate(new UserProfilePage());
            }
        }

        private void EnterKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (btnSave.IsEnabled)
                {
                    ClickSave(btnSave, null);
                }
                if (sender == txtCurrentPassword)
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

        private bool HasUnsavedFields()
        {
            bool canGoBack = false;

            string currentPassword = txtCurrentPassword.Password.Trim();
            string password = txtPassword.Password.Trim();
            string passwordConfirm = txtPasswordConfirm.Password.Trim();

            if (string.IsNullOrEmpty(currentPassword) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(passwordConfirm))
            {
                canGoBack = true;
            }

            return canGoBack;
        }

        private bool FieldsValidation(string currentPassword, string newPassword, string confirmPassword)
        {
            ClearAllErrors();
            bool areValid = true;

            if (string.IsNullOrEmpty(currentPassword))
            {
                ShowError(txtCurrentPassword, Lang.GlobalTextRequieredField);
                areValid = false;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ShowError(txtPassword, Lang.GlobalTextRequieredField);
                areValid = false;
            }

            if (string.IsNullOrEmpty(confirmPassword))
            {
                ShowError(txtPasswordConfirm, Lang.GlobalTextRequieredField);
                areValid = false;
            }

            if (!areValid)
            {
                return false;
            }

            if (currentPassword.Length < 8)
            {
                ShowError(txtCurrentPassword, Lang.DialogTextShortPassword);
                areValid = false;
            }
            else if (currentPassword.Length > 50)
            {
                ShowError(txtCurrentPassword, Lang.DialogTextLongPassword);
                areValid = false;
            }

            if (newPassword.Length < 12)
            {
                ShowError(txtPassword, Lang.DialogTextShortPassword);
                areValid = false;
            }
            else if (newPassword.Length > 50)
            {
                ShowError(txtPassword, Lang.DialogTextLongPassword);
                areValid = false;
            }
            else if (!IsPasswordComplex(newPassword))
            {
                string errorMessage = Lang.GlobalTextPasswordNoComplex;
                ShowError(txtPassword, errorMessage);
                ShowError(txtPasswordConfirm, errorMessage);
                areValid = false;
            }

            if (!string.Equals(newPassword, confirmPassword))
            {
                string errorMessage = Lang.DialogTextPasswordsDontMatch;
                ShowError(txtPassword, errorMessage);
                ShowError(txtPasswordConfirm, errorMessage);
                areValid = false;
            }

            if (string.Equals(currentPassword, newPassword))
            {
                ShowError(txtPassword, Lang.DialogTextPasswordSameAsOld);
                areValid = false;
            }

            CheckFormStatusAndToggleRegisterButton();
            return areValid;
        }

        private bool IsPasswordComplex(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

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

        private TextBlock GetErrorTextBlock(Control field)
        {
            TextBlock errorBlock = null;

            if (field == txtCurrentPassword)
            {
                errorBlock = blckCurrentError;
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
                errorBlock.Text = string.Empty;
            }

            field.ClearValue(Border.BorderBrushProperty);
        }

        private void ClearAllErrors()
        {
            ClearSpecificError(txtCurrentPassword);
            ClearSpecificError(txtPassword);
            ClearSpecificError(txtPasswordConfirm);
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

            if (password.Length < PASSWORD_MIN_LENGTH)
            {
                ShowError(passwordBox, Lang.DialogTextShortPassword);
            }
            else if (password.Length > PASSWORD_MAX_LENGTH)
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
            bool hasErrorMessages = blckCurrentError.Text.Trim().Length > MIN_LENGTH ||
                                    blckPasswordError.Text.Trim().Length > MIN_LENGTH ||
                                    blckPasswordConfirmError.Text.Trim().Length > MIN_LENGTH;

            bool allFieldsFilled = !string.IsNullOrWhiteSpace(txtCurrentPassword.Password) &&
                                   !string.IsNullOrWhiteSpace(txtPassword.Password) &&
                                   !string.IsNullOrWhiteSpace(txtPasswordConfirm.Password);

            bool canEnable = !hasErrorMessages && allFieldsFilled;

            btnSave.IsEnabled = canEnable;
        }

        private void ClickToggleVisibility(object sender, RoutedEventArgs e)
        {
            if (sender == btnToggleVisibilityCurrent)
            {
                if (txtCurrentPassword.Visibility == Visibility.Visible)
                {
                    txtVisibleCurrentPassword.Text = txtCurrentPassword.Password;

                    txtCurrentPassword.Visibility = Visibility.Collapsed;
                    txtVisibleCurrentPassword.Visibility = Visibility.Visible;
                    txtVisibleCurrentPassword.Focus();

                    blckEyeEmojiCurrent.Foreground = new SolidColorBrush(Colors.White);
                    txtVisibleCurrentPassword.BorderBrush = txtPassword.BorderBrush;
                }
                else
                {
                    txtCurrentPassword.Password = txtVisibleCurrentPassword.Text;

                    txtCurrentPassword.Visibility = Visibility.Visible;
                    txtVisibleCurrentPassword.Visibility = Visibility.Collapsed;
                    txtCurrentPassword.Focus();

                    blckEyeEmojiCurrent.Foreground = new SolidColorBrush(Colors.Black);
                    txtVisibleCurrentPassword.BorderBrush = txtPassword.BorderBrush;
                }
                return;
            }
            
            if (sender == btnToggleVisibility)
            {
                if (txtPassword.Visibility == Visibility.Visible)
                {
                    txtVisiblePassword.Text = txtPassword.Password;

                    txtPassword.Visibility = Visibility.Collapsed;
                    txtVisiblePassword.Visibility = Visibility.Visible;
                    txtVisiblePassword.Focus();

                    blckEyeEmoji.Foreground = new SolidColorBrush(Colors.White);
                    txtCurrentPassword.BorderBrush = txtPassword.BorderBrush;
                }
                else
                {
                    txtPassword.Password = txtVisiblePassword.Text;

                    txtPassword.Visibility = Visibility.Visible;
                    txtVisiblePassword.Visibility = Visibility.Collapsed;
                    txtPassword.Focus();

                    blckEyeEmoji.Foreground = new SolidColorBrush(Colors.Black);
                    txtCurrentPassword.BorderBrush = txtPassword.BorderBrush;
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
