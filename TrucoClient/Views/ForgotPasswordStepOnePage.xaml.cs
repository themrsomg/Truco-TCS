using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;

namespace TrucoClient.Views
{
    public partial class ForgotPasswordStepOnePage : Page
    {
        private const int MIN_EMAIL_LENGTH = 5;
        private const int MAX_EMAIL_LENGTH = 250;
        private const string MESSAGE_ERROR = "Error";

        public ForgotPasswordStepOnePage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void ClickSendCode(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            ErrorDisplayService.ClearError(txtEmail, blckEmailError);
            if (!FieldsValidation(email))
            {
                return;
            }

            btnSendCode.IsEnabled = false;

            try
            {
                var userClient = ClientManager.UserClient;

                bool exists = await userClient.EmailExistsAsync(email);
                if (!exists)
                {
                    ErrorDisplayService.ShowError(txtEmail, blckEmailError, Lang.GlobalTextEmailDoesntExist);
                    return;
                }

                string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                bool sent = await userClient.RequestEmailVerificationAsync(email, languageCode);

                if (sent)
                {
                    CustomMessageBox.Show(Lang.ForgotPasswordTextSent2, Lang.ForgotPasswordRecovery, 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.NavigationService.Navigate(new ForgotPasswordStepTwoPage(email));
                }
                else
                {
                    CustomMessageBox.Show(Lang.ForgotPasswordTextError2, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
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
            finally
            {
                if (this.IsLoaded)
                {
                    btnSendCode.IsEnabled = true;
                }
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void EnterKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender == txtEmail)
            {
                ClickSendCode(btnSendCode, null);
                e.Handled = true;
            }
        }

        private bool FieldsValidation(string email)
        {
            bool areValid = true;

            if (!FieldValidator.IsRequired(email))
            {
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, Lang.GlobalTextRequieredField);
                areValid = false;
            }
            else if (!FieldValidator.IsLengthInRange(email, MIN_EMAIL_LENGTH, MAX_EMAIL_LENGTH))
            {
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, email.Length < MIN_EMAIL_LENGTH ? Lang.DialogTextShortEmail : Lang.DialogTextLongEmail);
                areValid = false;
            }
            else if (!EmailValidator.IsValidEmail(email))
            {
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, Lang.GlobalTextInvalidEmail);
                areValid = false;
            }

            return areValid;
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            ErrorDisplayService.ClearError(txtEmail, blckEmailError);
            string text = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (!FieldValidator.IsLengthInRange(text, MIN_EMAIL_LENGTH, MAX_EMAIL_LENGTH))
            {
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, text.Length < MIN_EMAIL_LENGTH ? Lang.DialogTextShortEmail : Lang.DialogTextLongEmail);
            }
            else if (!EmailValidator.IsValidEmail(text))
            {
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, Lang.GlobalTextInvalidEmail);
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, Lang.GlobalTextRequieredField);
            }
        }
    }
}