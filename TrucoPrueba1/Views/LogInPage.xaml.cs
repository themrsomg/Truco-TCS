using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrucoPrueba1.Properties.Langs;
using TrucoPrueba1.Views;

namespace TrucoPrueba1
{
    public partial class LogInPage : Page
    {
        private string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        public LogInPage()
        {
            InitializeComponent();
            SessionManager.ClearSession();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void ClickLogIn(object sender, RoutedEventArgs e)
        {
            string emailOrUsername = txtEmailUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (!FieldsValidation(emailOrUsername, password))
            {
                return;
            }

            btnLogIn.IsEnabled = false;

            try
            {
                var userClient = ClientManager.UserClient;

                bool success = await userClient.LoginAsync(emailOrUsername, password, languageCode);

                if (success)
                {
                    string resolvedUsername = await SessionManager.ResolveUsernameAsync(emailOrUsername);
                    SessionManager.CurrentUsername = resolvedUsername;
                    SessionManager.CurrentUserData = await userClient.GetUserProfileAsync(resolvedUsername);

                    MessageBox.Show(Lang.GlobalTextWelcome + " " + SessionManager.CurrentUsername + "!", Lang.GlobalTextWelcome + "!", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.NavigationService.Navigate(new MainPage());
                }
                else
                {
                    ShowError(txtEmailUsername, Lang.DialogTextInvalidUserPass);
                    ShowError(txtPassword, Lang.DialogTextInvalidUserPass);
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
                    btnLogIn.IsEnabled = true;
                }
            }
        }

        private void ClickForgotPassword(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ForgotPasswordStepOnePage()); 
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new StartPage());
        }

        private void EnterKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == txtEmailUsername)
                {
                    txtPassword.Focus();
                    e.Handled = true;
                }
                else if (sender == txtPassword)
                {
                    ClickLogIn(btnLogIn, null);
                    e.Handled = true;
                }
            }
        }

        private bool FieldsValidation (string emailOrUsername, string password)
        {
            ClearAllErrors();
            bool areValid = true;

            if (string.IsNullOrEmpty(emailOrUsername))
            {
                ShowError(txtEmailUsername, Lang.GlobalTextRequieredField);
                areValid = false;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError(txtPassword, Lang.GlobalTextRequieredField);
                areValid = false;
            }

            return areValid;
        }

        private TextBlock GetErrorTextBlock(Control field)
        {
            TextBlock errorBlock = null;

            if (field == txtEmailUsername)
            {
                errorBlock = blckEmailUsernameError;
            }
            if (field == txtPassword)
            {
                errorBlock = blckPasswordError;
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
            ClearSpecificError(txtEmailUsername);
            ClearSpecificError(txtPassword);
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            ClearSpecificError(textBox);

            string text = textBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (textBox == txtEmailUsername)
            {
                if (text.Length < 4)
                {
                    ShowError(txtEmailUsername, Lang.DialogTextShortEmail);
                }
                else if (text.Length > 250)
                {
                    ShowError(txtEmailUsername, Lang.DialogTextLongEmail);
                }
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                ShowError(textBox, Lang.GlobalTextRequieredField);
            }
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
        }

        private void PasswordLostFocus(object sender, RoutedEventArgs e)
        {
            ClearSpecificError(txtPassword);
            
            var passwordBox = sender as PasswordBox;

            if (string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                ShowError(passwordBox, Lang.GlobalTextRequieredField);
            }
        }
    }
}
