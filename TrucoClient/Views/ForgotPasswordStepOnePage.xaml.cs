using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.Properties.Langs;

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
            ApplyInputSanitization();
        }

        private async void ClickSendCode(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            ErrorDisplayService.ClearError(txtEmail, blckEmailError);

            if (!ValidateEmailField(email))
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
            catch (FaultException ex)
            {
                ClientException.HandleError(ex, nameof(ClickSendCode));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (EndpointNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(ClickSendCode));
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError,
                    Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                ClientException.HandleError(ex, nameof(ClickSendCode));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException ex)
            {
                ClientException.HandleError(ex, nameof(ClickSendCode));
                CustomMessageBox.Show(Lang.ExceptionTextTimeout,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(ClickSendCode));
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

        private void ApplyInputSanitization()
        {
            const string emailPattern = @"^(?!.*@.*@)[a-zA-Z0-9._%+\-@]*$";

            try
            {
                InputRestriction.AttachTextBoxValidation(txtEmail, emailPattern);
            }
            catch (ArgumentNullException ex)
            {
                ClientException.HandleError(ex, nameof(ApplyInputSanitization));
            }
            catch (ArgumentException ex)
            {
                ClientException.HandleError(ex, nameof(ApplyInputSanitization));
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(ApplyInputSanitization));
            }
        }

        private void EmailPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ErrorDisplayService.ClearError(txtEmail, blckEmailError);
        }

        private void EmailPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ClickSendCode(btnSendCode, null);
                e.Handled = true;
            }
        }

        private void EmailPastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            ErrorDisplayService.ClearError(txtEmail, blckEmailError);
        }

        private bool ValidateEmailField(string email)
        {
            if (!FieldValidator.IsRequired(email))
            {
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, Lang.GlobalTextRequieredField);
                return false;
            }

            if (!FieldValidator.IsLengthInRange(email, MIN_EMAIL_LENGTH, MAX_EMAIL_LENGTH))
            {
                string msg = email.Length < MIN_EMAIL_LENGTH ? Lang.DialogTextShortEmail : Lang.DialogTextLongEmail;
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, msg);
                return false;
            }

            if (!EmailValidator.IsValidEmail(email))
            {
                ErrorDisplayService.ShowError(txtEmail, blckEmailError, Lang.GlobalTextInvalidEmail);
                return false;
            }

            return true;
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