using System;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Localization;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class LogInPage : Page
    {
        private const string MESSAGE_ERROR = "Error";
        private const int MIN_TEXT_LENGTH = 4;
        private const int MAX_TEXT_LENGTH = 250;
        private const int MIN_PASSWORD_LENGTH = 12;
        private const int MAX_PASSWORD_LENGTH = 50;
        private const int LOGIN_DELAY_MS = 5000;

        private static readonly Regex loginAllowedRegex = new Regex(@"^[a-zA-Z0-9@._+-]+$", RegexOptions.Compiled);
        private static readonly Regex passwordAllowedRegex = new Regex(@"^[^\s]+$", RegexOptions.Compiled);
        private readonly string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        public LogInPage()
        {
            InitializeComponent();
            SessionManager.Clear();
            InitializeValidation();
            MusicInitializer.InitializeMenuMusic();
        }

        private void InitializeValidation()
        {
            InputRestriction.AttachRegexValidation(txtEmailUsername, loginAllowedRegex);
            InputRestriction.AttachRegexValidation(txtPassword, passwordAllowedRegex);
        }

        private async void ClickLogIn(object sender, RoutedEventArgs e)
        {
            string identifier = txtEmailUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (!AreFieldsValid(identifier, password))
            {
                return;
            }

            btnLogIn.IsEnabled = false;
            
            await AttemptLoginAsync(identifier, password);
        }

        private async Task AttemptLoginAsync(string identifier, string password)
        {
            try
            {
                ClientManager.ResetConnections();
                var userClient = ClientManager.UserClient;

                bool success = await userClient.LoginAsync(identifier, password, languageCode);

                if (!success)
                {
                    ShowInvalidCredentialsError();
                    return;
                }

                await ProcessSuccessfulLogin(identifier, userClient);
            }
            catch (FaultException<LoginFault> ex)
            {
                await HandleLoginFault(ex);
            }
            catch (FaultException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextUserAlreadyLoggedIn, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (EndpointNotFoundException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError, Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorLoggingIn, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (this.IsLoaded)
                {
                    btnLogIn.IsEnabled = true;
                }
            }
        }

        private async Task ProcessSuccessfulLogin(string identifier, ITrucoUserService userClient)
        {
            string username = await SessionManager.ResolveUsernameAsync(identifier);
            SessionManager.CurrentUsername = username;
            var userData = await userClient.GetUserProfileAsync(username);
            SessionManager.CurrentUserData = userData;

            if (!string.IsNullOrEmpty(userData.LanguageCode))
            {
                Properties.Settings.Default.languageCode = userData.LanguageCode;
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(userData.LanguageCode);
                LanguageManager.ApplyLanguage();
            }

            if (MusicManager.IsMuted != userData.IsMusicMuted)
            {
                MusicManager.ToggleMute();
            }
            
            Properties.Settings.Default.IsMusicMuted = userData.IsMusicMuted;
            Properties.Settings.Default.Save();

            CustomMessageBox.Show($"{Lang.GlobalTextWelcome} {username}!", Lang.GlobalTextWelcome,
                MessageBoxButton.OK, MessageBoxImage.Information);

            this.NavigationService.Navigate(new MainPage());
        }

        private async Task HandleLoginFault(FaultException<LoginFault> ex)
        {
            if (ex.Detail.ErrorCode == "TooManyAttempts")
            {
                CustomMessageBox.Show(Lang.ExceptionTextTooManyAttempts, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                btnLogIn.IsEnabled = false;
                await Task.Delay(LOGIN_DELAY_MS);

                return;
            }

            if (ex.Detail.ErrorCode == "UserAlreadyLoggedIn")
            {
                CustomMessageBox.Show(Lang.ExceptionTextUserAlreadyLoggedIn, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorLoggingIn, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowInvalidCredentialsError()
        {
            ErrorDisplayService.ShowError(txtEmailUsername, blckEmailUsernameError, Lang.DialogTextInvalidUserPass);
            ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextInvalidUserPass);
        }

        private bool AreFieldsValid(string identifier, string password)
        {
            ClearErrors();
            bool isIdentifierValid = ValidateIdentifierField(identifier);
            bool isPasswordValid = ValidatePasswordField(password);

            return isIdentifierValid && isPasswordValid;
        }

        private bool ValidateIdentifierField(string identifier)
        {
            if (!FieldValidator.IsRequired(identifier))
            {
                ErrorDisplayService.ShowError(txtEmailUsername, blckEmailUsernameError, Lang.GlobalTextRequieredField);
                
                return false;
            }

            if (InputSanitizer.ContainsDangerousCharacters(identifier))
            {
                ErrorDisplayService.ShowError(txtEmailUsername, blckEmailUsernameError, Lang.DialogTextInvalidCharacters);
                
                return false;
            }

            return ValidateIdentifierFormat(identifier, txtEmailUsername, blckEmailUsernameError);
        }

        private bool ValidateIdentifierFormat(string text, TextBox control, TextBlock errorBlock)
        {
            if (text.Contains("@"))
            {
                if (!EmailValidator.IsValidEmail(text))
                {
                    ErrorDisplayService.ShowError(control, errorBlock, Lang.DialogTextInvalidEmail);
                    
                    return false;
                }
            }
            else
            {
                if (!UsernameValidator.ValidateLength(text, MIN_TEXT_LENGTH, MAX_TEXT_LENGTH) ||
                    !UsernameValidator.IsValidFormat(text))
                {
                    ErrorDisplayService.ShowError(control, errorBlock, Lang.DialogTextInvalidUsername);
                    
                    return false;
                }
            }
            return true;
        }

        private bool ValidatePasswordField(string password)
        {
            if (!FieldValidator.IsRequired(password))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.GlobalTextRequieredField);
                
                return false;
            }

            if (InputSanitizer.ContainsDangerousCharacters(password))
            {
                ErrorDisplayService.ShowError(txtPassword, blckPasswordError, Lang.DialogTextInvalidCharacters);
                
                return false;
            }

            return ValidatePasswordFormat(password, txtPassword, blckPasswordError);
        }

        private bool ValidatePasswordFormat(string password, PasswordBox control, TextBlock errorBlock)
        {
            if (!PasswordValidator.ValidateLength(password, MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH))
            {
                ErrorDisplayService.ShowError(control, errorBlock, Lang.DialogTextShortPassword);
                
                return false;
            }

            if (!PasswordValidator.IsComplex(password))
            {
                ErrorDisplayService.ShowError(control, errorBlock, Lang.DialogTextInvalidPassword);
                
                return false;
            }

            return true;
        }

        private void ClearErrors()
        {
            ErrorDisplayService.ClearError(txtEmailUsername, blckEmailUsernameError);
            ErrorDisplayService.ClearError(txtPassword, blckPasswordError);
        }

        private void ClickForgotPassword(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ForgotPasswordStepOnePage());
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new StartPage());
        }

        private void ClickToggleVisibility(object sender, RoutedEventArgs e)
        {
            PasswordVisibilityService.ToggleVisibility(txtPassword, txtVisiblePassword, blckEyeEmoji);
            SyncBorderBrushes();
        }

        private void SyncBorderBrushes()
        {
            Control visibleBox = txtVisiblePassword.Visibility == Visibility.Visible ? (Control)txtVisiblePassword : (Control)txtPassword;
            Control hiddenBox = visibleBox == txtVisiblePassword ? (Control)txtPassword : (Control)txtVisiblePassword;
            visibleBox.BorderBrush = hiddenBox.BorderBrush;
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox == null)
            {
                return;
            }

            ErrorDisplayService.ClearError(textBox, blckEmailUsernameError);
            string text = textBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            ValidateRealTimeLength(text, textBox);
        }

        private void ValidateRealTimeLength(string text, TextBox textBox)
        {
            if (!FieldValidator.IsLengthInRange(text, MIN_TEXT_LENGTH, MAX_TEXT_LENGTH))
            {
                string error;
                
                if (text.Length < MIN_TEXT_LENGTH)
                {
                    error = Lang.DialogTextShortEmailOrUsername;
                }
                else
                {
                    error = text.Contains("@") ? Lang.DialogTextLongEmail : Lang.DialogTextLongUsername;
                }
                ErrorDisplayService.ShowError(textBox, blckEmailUsernameError, error);
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
           
            if (textBox == null)
            {
                return;
            }

            string text = textBox.Text.Trim();
            ErrorDisplayService.ClearError(textBox, blckEmailUsernameError);

            ValidateIdentifierField(text);
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            if (passwordBox == null)
            {
                return;
            }

            ErrorDisplayService.ClearError(passwordBox, blckPasswordError);
            string password = passwordBox.Password.Trim();

            if (string.IsNullOrEmpty(password))
            {
                return;
            }

            ValidatePasswordLengthRealTime(password, passwordBox);
            SyncVisiblePassword(passwordBox);
        }

        private void ValidatePasswordLengthRealTime(string password, PasswordBox passwordBox)
        {
            if (!PasswordValidator.ValidateLength(password, MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH))
            {
                string error = password.Length < MIN_PASSWORD_LENGTH ? Lang.DialogTextShortPassword : Lang.DialogTextLongPassword;
                ErrorDisplayService.ShowError(passwordBox, blckPasswordError, error);
            }
        }

        private void SyncVisiblePassword(PasswordBox passwordBox)
        {
            if (passwordBox == txtPassword && txtVisiblePassword.Visibility == Visibility.Visible)
            {
                txtVisiblePassword.Text = txtPassword.Password;
            }
        }

        private void PasswordLostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            if (passwordBox == null)
            {
                return;
            }

            string password = passwordBox.Password.Trim();
            ErrorDisplayService.ClearError(passwordBox, blckPasswordError);

            ValidatePasswordField(password);
        }

        private void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandleEnterKey(sender);
            }
        }

        private void HandleEnterKey(object sender)
        {
            if (sender == txtEmailUsername)
            {
                txtPassword.Focus();
            }
            else if (sender == txtPassword)
            {
                ClickLogIn(btnLogIn, null);
            }
        }
    }
}