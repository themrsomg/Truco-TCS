using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrucoPrueba1.Properties.Langs;

namespace TrucoPrueba1.Views
{
    public partial class ChangePasswordPage : Page
    {
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
                MessageBox.Show($"No se pudo conectar al servidor: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (newPassword.Length < 8)
            {
                ShowError(txtPassword, Lang.DialogTextShortPassword);
                areValid = false;
            }
            else if (newPassword.Length > 50)
            {
                ShowError(txtPassword, Lang.DialogTextLongPassword);
                areValid = false;
            }

            if (!string.Equals(newPassword, confirmPassword))
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
                errorBlock.Text = " ";
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
            bool hasErrorMessages = blckCurrentError.Text.Trim().Length > 0 ||
                                    blckPasswordError.Text.Trim().Length > 0 ||
                                    blckPasswordConfirmError.Text.Trim().Length > 0;

            bool allFieldsFilled = !string.IsNullOrWhiteSpace(txtCurrentPassword.Password) &&
                                   !string.IsNullOrWhiteSpace(txtPassword.Password) &&
                                   !string.IsNullOrWhiteSpace(txtPasswordConfirm.Password);

            bool canEnable = !hasErrorMessages && allFieldsFilled;

            btnSave.IsEnabled = canEnable;
        }
    }
}
