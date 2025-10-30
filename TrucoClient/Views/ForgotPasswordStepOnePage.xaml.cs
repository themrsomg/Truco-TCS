using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class ForgotPasswordStepOnePage : Page
    {

        private const int MIN_EMAIL_LENGTH = 5;
        private const int MAX_EMAIL_LENGTH = 250;
        private const int MIN_TEXT_LENGTH = 5;
        private const int MAX_TEXT_LENGTH = 250;

        public ForgotPasswordStepOnePage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void ClickSendCode(object sender, RoutedEventArgs e)
        {
           string email = txtEmail.Text.Trim();

            if (!FieldsValidation(email))
            {
                return;
            }

            btnSendCode.IsEnabled = false;

            try
            {
                var userClient = ClientManager.UserClient;

                if (await EmailVerificationAsync(email, userClient))
                {
                    return;
                }

                string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

                bool sent = await userClient.RequestEmailVerificationAsync(email, languageCode);

                if (sent)
                {
                    MessageBox.Show(Lang.ForgotPasswordTextSent2, Lang.ForgotPasswordRecovery, MessageBoxButton.OK, MessageBoxImage.Information);
                    this.NavigationService.Navigate(new ForgotPasswordStepTwoPage(email));
                }
                else
                {
                    MessageBox.Show(Lang.ForgotPasswordTextError2, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            if (e.Key == Key.Enter)
            {
                if (sender == txtEmail)
                {
                    ClickSendCode(btnSendCode, null);
                    e.Handled = true;
                }
            }
        }

        private bool FieldsValidation(string email)
        {
            ClearError();
            bool areValid = true;

            if (string.IsNullOrEmpty(email))
            {
                ShowError(Lang.GlobalTextRequieredField);
                areValid = false;
            }

            if (!areValid)
            {
                return false;
            }

            if (email.Length < MIN_EMAIL_LENGTH)
            {
                ShowError(Lang.DialogTextShortEmail);
                areValid = false;
            }
            else if (email.Length > MAX_EMAIL_LENGTH)
            {
                ShowError(Lang.DialogTextLongEmail);
                areValid = false;
            }
            else if (!IsValidEmail(email))
            {
                ShowError(Lang.GlobalTextInvalidEmail);
                areValid = false;
            }

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

        private async Task<bool> EmailVerificationAsync(string email, TrucoUserServiceClient userClient)
        {
            bool emailExists = await userClient.EmailExistsAsync(email);

            if (!emailExists)
            {
                ShowError(Lang.GlobalTextEmailDoesntExist);
                emailExists = true;
            }

            emailExists = false;
            return emailExists;
        }

        private void ShowError(string errorMessage)
        {
            blckEmailError.Text = errorMessage;

            txtEmail.BorderBrush = new SolidColorBrush(Colors.Red);
        }

        private void ClearError()
        {
            blckEmailError.Text = string.Empty;

            txtEmail.ClearValue(Border.BorderBrushProperty);
        }
        
        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            ClearError();
            string text = textBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (textBox == txtEmail)
            {
                if (text.Length < MIN_TEXT_LENGTH)
                {
                    ShowError(Lang.DialogTextShortEmail);
                }
                else if (text.Length > MAX_TEXT_LENGTH)
                {
                    ShowError(Lang.DialogTextLongEmail);
                }
                else if (!IsValidEmail(text))
                {
                    ShowError(Lang.GlobalTextInvalidEmail);
                }
            }
        }
        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowError(Lang.GlobalTextRequieredField);
            }
        }
    }
}
