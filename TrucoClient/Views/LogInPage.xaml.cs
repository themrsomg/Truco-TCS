using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using TrucoClient.Properties.Langs;
using TrucoClient.Views;

namespace TrucoClient
{
    public partial class LogInPage : Page
    {
        private const int MIN_TEXT_LENGTH = 4;
        private const int MAX_TEXT_LENGTH = 250;
        private const int MIN_PASSWORD_LENGTH = 12;
        private const int MAX_PASSWORD_LENGTH = 50;

        private string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        public LogInPage()
        {
            InitializeComponent();
            SessionManager.Clear();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void ClickLogIn(object sender, RoutedEventArgs e)
        {
            string emailOrUsername = txtEmailUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(emailOrUsername) || string.IsNullOrEmpty(password))
            {
                ShowError(txtEmailUsername, Lang.GlobalTextRequieredField);
                ShowError(txtPassword, Lang.GlobalTextRequieredField);
                return;
            }

            btnLogIn.IsEnabled = false;

            try
            {
                ClientManager.ResetConnections();
                var userClient = ClientManager.UserClient;

                bool success = await userClient.LoginAsync(emailOrUsername, password, languageCode);

                if (success)
                {
                    string resolvedUsername = await SessionManager.ResolveUsernameAsync(emailOrUsername);
                    SessionManager.CurrentUsername = resolvedUsername;
                    SessionManager.CurrentUserData = await userClient.GetUserProfileAsync(resolvedUsername);

                    MessageBox.Show(Lang.GlobalTextWelcome + " " + SessionManager.CurrentUsername + "!", Lang.GlobalTextWelcome + "!",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                    this.NavigationService.Navigate(new MainPage());
                }
                else
                {
                    ShowError(txtEmailUsername, Lang.DialogTextInvalidUserPass);
                    ShowError(txtPassword, Lang.DialogTextInvalidUserPass);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar sesión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnLogIn.IsEnabled = true;
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
                errorBlock.Text = string.Empty;
            }

            field.ClearValue(Border.BorderBrushProperty);
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
                if (text.Length < MIN_TEXT_LENGTH)
                {
                    ShowError(txtEmailUsername, Lang.DialogTextShortEmailOrUsername);
                }
                else if (text.Length > MAX_TEXT_LENGTH)
                {
                    if (text.Contains("@"))
                    {
                        ShowError(txtEmailUsername, Lang.DialogTextLongEmail);
                    }
                    else
                    {
                        ShowError(txtEmailUsername, Lang.DialogTextLongUsername);
                    }
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

            if (password.Length < MIN_PASSWORD_LENGTH)
            {
                ShowError(passwordBox, Lang.DialogTextShortPassword);
            }
            else if (password.Length > MAX_PASSWORD_LENGTH)
            {
                ShowError(passwordBox, Lang.DialogTextLongPassword);
            }

            if (passwordBox == txtPassword && txtVisiblePassword.Visibility == Visibility.Visible)
            {
                txtVisiblePassword.Text = txtPassword.Password;
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

        private void ClickToggleVisibility(object sender, RoutedEventArgs e)
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
        }
    }
}
