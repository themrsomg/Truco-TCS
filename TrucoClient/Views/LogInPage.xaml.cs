using System;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class LogInPage : Page
    {
        private const string MESSAGE_ERROR = "Error";
        private const int MIN_TEXT_LENGTH = 4;
        private const int MAX_TEXT_LENGTH = 250;
        private const int MIN_PASSWORD_LENGTH = 12;
        private const int MAX_PASSWORD_LENGTH = 50;

        private readonly string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        public LogInPage()
        {
            InitializeComponent();
            SessionManager.Clear();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void ClickLogIn(object sender, RoutedEventArgs e)
        {
            string identifier = txtEmailUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (!FieldsValidation(identifier, password))
            {
                return;
            }

            btnLogIn.IsEnabled = false;

            try
            {
                ClientManager.ResetConnections();
                var userClient = ClientManager.UserClient;

                bool success = await userClient.LoginAsync(identifier, password, languageCode);

                if (!success)
                {
                    ErrorDisplayService.ShowError(txtEmailUsername, blckEmailUsernameError, Lang.DialogTextInvalidUserPass);
                    ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextInvalidUserPass);
                    return;
                }

                string username = await SessionManager.ResolveUsernameAsync(identifier);
                SessionManager.CurrentUsername = username;
                SessionManager.CurrentUserData = await userClient.GetUserProfileAsync(username);

                CustomMessageBox.Show($"{Lang.GlobalTextWelcome} {username}!", Lang.GlobalTextWelcome, 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.NavigationService.Navigate(new MainPage());
            }
            catch (FaultException<TrucoServer.LoginFault> ex)
            {
                if (ex.Detail.ErrorCode == "UserAlreadyLoggedIn")
                {
                    CustomMessageBox.Show(Lang.DialogTextUserAlreadyLoggedIn,
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    CustomMessageBox.Show(Lang.ExceptionTextErrorLoggingIn,
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (FaultException)
            {
                CustomMessageBox.Show(Lang.DialogTextUserAlreadyLoggedIn,
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (EndpointNotFoundException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError, 
                    Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorLoggingIn, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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

        private bool FieldsValidation(string identifier, string password)
        {
            bool areValid = true;
            ClearErrors();

            if (!FieldValidator.IsRequired(identifier))
            {
                ErrorDisplayService.ShowError(txtEmailUsername, blckEmailUsernameError, Lang.GlobalTextRequieredField);
                areValid = false;
            }

            if (!FieldValidator.IsRequired(password))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.GlobalTextRequieredField);
                areValid = false;
            }

            return areValid;
        }

        private void ClearErrors()
        {
            ErrorDisplayService.ClearError(txtEmailUsername, blckEmailUsernameError);
            ErrorDisplayService.ClearError(txtPassword, blckPasswordError);
        }

        private void ClickToggleVisibility(object sender, RoutedEventArgs e)
        {
            PasswordVisibilityService.ToggleVisibility(txtPassword, txtVisiblePassword, blckEyeEmoji);

            Control visibleBox = txtVisiblePassword.Visibility == Visibility.Visible ? (Control)txtVisiblePassword : (Control)txtPassword;
            Control hiddenBox = visibleBox == txtVisiblePassword ? (Control)txtPassword : (Control)txtVisiblePassword;
            visibleBox.BorderBrush = hiddenBox.BorderBrush;
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            ErrorDisplayService.ClearError(textBox, blckEmailUsernameError);
            string text = textBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (!FieldValidator.IsLengthInRange(text, MIN_TEXT_LENGTH, MAX_TEXT_LENGTH))
            {
                string error;

                if (text.Length < MIN_TEXT_LENGTH)
                {
                    error = Lang.DialogTextShortEmailOrUsername;
                }
                else
                {
                    if (text.Contains("@"))
                    {
                        error = Lang.DialogTextLongEmail;
                    }
                    else
                    {
                        error = Lang.DialogTextLongUsername;
                    }
                }

                ErrorDisplayService.ShowError(textBox, blckEmailUsernameError, error);
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (!FieldValidator.IsRequired(textBox.Text))
            {
                ErrorDisplayService.ShowError(textBox, blckEmailUsernameError, Lang.GlobalTextRequieredField);
            }
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            ErrorDisplayService.ClearError(passwordBox, blckPasswordError);
            string password = passwordBox.Password.Trim();

            if (string.IsNullOrEmpty(password))
            {
                return;
            }

            if (!PasswordValidator.ValidateLength(password, MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH))
            {
                string error = password.Length < MIN_PASSWORD_LENGTH ? Lang.DialogTextShortPassword : Lang.DialogTextLongPassword;
                ErrorDisplayService.ShowError(passwordBox, blckPasswordError, error);
            }

            if (passwordBox == txtPassword && txtVisiblePassword.Visibility == Visibility.Visible)
            {
                txtVisiblePassword.Text = txtPassword.Password;
            }
        }

        private void PasswordLostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            ErrorDisplayService.ClearError(passwordBox, blckPasswordError);

            if (!FieldValidator.IsRequired(passwordBox.Password))
            {
                ErrorDisplayService.ShowError(passwordBox, blckPasswordError, Lang.GlobalTextRequieredField);
            }
        }

        private void UsernamePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$", RegexOptions.None, TimeSpan.FromMilliseconds(100));
        }
    }
}